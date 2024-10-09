using BigBoxAutoPlay.AutoPlayers;
using BigBoxAutoPlay.DataAccess;
using BigBoxAutoPlay.Helpers;
using BigBoxAutoPlay.Models;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;
using Unbroken.LaunchBox.Plugins.RetroAchievements;


namespace BigBoxAutoPlay
{
    public class BigBoxAutoPlayPlugin : ISystemEventsPlugin, IGameLaunchingPlugin
    {        
        private static Thread listenerThread;
        private static UdpClient udpServer;
        private static IPEndPoint serverEndpoint;
        private static IPEndPoint remoteEndpoint;
        private static Dispatcher dispatcher;
        private static bool playingGame;
        private static bool threadEnabled;

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public void OnEventRaised(string eventType)
        {
            try
            {
                switch (eventType)
                {
                    case SystemEventTypes.PluginInitialized:
                        dispatcher = Dispatcher.CurrentDispatcher;
                        break;

                    case SystemEventTypes.BigBoxStartupCompleted:
                        CreateServer();
                        DelayThenAutoPlay();
                        break;

                    case SystemEventTypes.BigBoxShutdownBeginning:
                        CloseServer();
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex, "OnEventRaised");
            }
        }

        private void CloseServer()
        {
            try
            {
                // Need to cleanup the spawned thread gracefully.
                // closing the connection should cause an emtpy byte to
                // be returned and the thread should exit the loop
                threadEnabled = false;

                udpServer?.Close();
                udpServer?.Dispose();
                udpServer = null;
                serverEndpoint = null;
                remoteEndpoint = null;

                // wait for the listener tread to finish
                listenerThread.Join(5000);
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex, "CloseServer");
            }
        }

        private void DelayThenAutoPlay()
        {
            BigBoxAutoPlaySettings bigBoxAutoPlaySettings = BigBoxAutoPlaySettingsDataService.Instance.GetSettings();

            if (bigBoxAutoPlaySettings?.DelayInSeconds.GetValueOrDefault() > 0)
            {
                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += DoBackgroundDelay;
                backgroundWorker.RunWorkerCompleted += DoAutoPlayOnStartup;
                backgroundWorker.RunWorkerAsync();
            }
            else
            {
                BigBoxAutoPlayer.AutoPlayOnStartup();
            }
        }

        private void DoBackgroundDelay(object sender, DoWorkEventArgs e)
        {
            try
            {
                BigBoxAutoPlaySettings bigBoxAutoPlaySettings = BigBoxAutoPlaySettingsDataService.Instance.GetSettings();                

                if (bigBoxAutoPlaySettings?.DelayInSeconds.GetValueOrDefault() > 0)
                {
                    Thread.Sleep(1000 * bigBoxAutoPlaySettings.DelayInSeconds.GetValueOrDefault());
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex, "DoBackgroundDelay");
            }
        }
        private void CreateServer()
        {
            BigBoxAutoPlaySettings bigBoxAutoPlaySettings = BigBoxAutoPlaySettingsDataService.Instance.GetSettings();

            if ( bigBoxAutoPlaySettings?.ServerEnable == true )
            {
                IPAddress ipAddress = null;
                int myPort        = bigBoxAutoPlaySettings.ServerPort.GetValueOrDefault();
                int remotePort  = bigBoxAutoPlaySettings.RemotePort.GetValueOrDefault();  

                try
                {
                    udpServer = new UdpClient(myPort);
                    serverEndpoint = new IPEndPoint(IPAddress.Any, myPort);

                    if (bigBoxAutoPlaySettings.MulticastEnable == true)
                    {
                        ipAddress = IPAddress.Parse(bigBoxAutoPlaySettings.MulticastAddress);
                        udpServer.JoinMulticastGroup(ipAddress);
                    } else
                    {
                        ipAddress = IPAddress.Parse(bigBoxAutoPlaySettings.RemoteIPAddress);
                    }

                    remoteEndpoint = new IPEndPoint(ipAddress, myPort);
                }
                catch (Exception ex)
                {
                    LogHelper.LogException(ex, "CreateServer");
                    return;
                }

                // listenerThread = new Thread(() => StartListener(ipAddress, port, dispatcher));
                threadEnabled = true;
                listenerThread = new Thread(() => StartListener());
                listenerThread.Start();
            }
        }

        private void DoAutoPlayOnStartup(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                BigBoxAutoPlayer.AutoPlayOnStartup();
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex, "DoAutoPlay");
            }
        }

        // static void StartListener(IPAddress ipAddress, int port, Dispatcher dispatcher)
        static void StartListener()
        {
            string receivedData = string.Empty;
            BigBoxAutoPlaySettings mySettings = BigBoxAutoPlaySettingsDataService.Instance.GetSettings();
            BigBoxAutoPlaySettings remoteSettings = null;

            while (threadEnabled)
            {
                // UDP receive
                try
                {
                    // Read data from the client by blocking
                    byte[] buffer = udpServer?.Receive(ref serverEndpoint);
                    receivedData = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                }
                catch
                {
                    // an exception will happen when we cancel the blocking
                    // operation on a call to UdpClient.close().  Its expected
                    // so ignore it
                    continue;
                }

                // Happy path...
                StringBuilder responseStringBuilder = new StringBuilder();
                responseStringBuilder.AppendLine($"Received message: {receivedData}");

                try
                {
                    remoteSettings = JsonConvert.DeserializeObject<BigBoxAutoPlaySettings>(receivedData);
                }
                catch (Exception ex)
                {
                    remoteSettings = null;
                    responseStringBuilder.AppendLine($"Error deserializing message: {ex.Message}");
                    LogHelper.LogException(ex, $"StartListener - deserializing message\n{receivedData}");
                }

                if (remoteSettings != null)
                {
                    if (mySettings.RemoteSync == true)
                    {
                        if (playingGame && remoteSettings.GameState == GameStateEnum.GAME_EXITED)
                        {
                            // For multiple Networked Computers running BigBox
                            // The Game has ended on the far end machine, end the
                            // game here by sending the escape key. 
                            //
                            //invoke via dispatcher so it runs under the BigBox UI thread
                            dispatcher.Invoke(() =>
                            {
                                ExitGame();
                            });
                        }
                        else if (!playingGame && remoteSettings.GameState == GameStateEnum.GAME_LAUNCHED)
                        {
                            try
                            {
                                // autoplay from the provided settings - invoke via dispatcher so it runs under the BigBox UI thread
                                dispatcher.Invoke(() =>
                                {
                                    responseStringBuilder.AppendLine(BigBoxAutoPlayer.AutoPlayFromMessage(remoteSettings));
                                });
                            }
                            catch (Exception ex)
                            {
                                // log the exception locally and send the response to the remote
                                LogHelper.LogException(ex, "StartListener - Autoplay");

                                responseStringBuilder.AppendLine($"Error playing from message: {ex.Message}");
                                SendResponse(responseStringBuilder.ToString());
                            }
                        }
                    }                    
                } 
                else
                {
                    //send to client for debug
                    SendResponse(responseStringBuilder.ToString());
                }
            }
        }

        private static void SendResponse(string response)
        {
            try
            {
                // send the response
                byte[] buffer = Encoding.ASCII.GetBytes(response);
                udpServer?.Send(buffer, buffer.Length, remoteEndpoint);
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex, "SendResponse");
            }
        }

        public void OnBeforeGameLaunching(IGame game, IAdditionalApplication app, IEmulator emulator)
        {
            // do nothing
        }

        public void OnAfterGameLaunched(IGame game, IAdditionalApplication app, IEmulator emulator)
        {
           
            BigBoxAutoPlaySettings settings = BigBoxAutoPlaySettingsDataService.Instance.GetSettings();

            // only send if AutoPlay is enabled
            if ( settings?.Enabled == true && settings?.ServerEnable == true && settings?.RemoteSync == true)
            {
                // This game may have been either launched by the user or
                // via autoplay.  In either case, alert the multicast group
                // that we just started a game.
                settings.FromPlaylist = "";
                settings.FromPlaylistName = "";
                settings.FromPlatform = "";
                settings.GameTitle = "";

                // limit how the base query in the remote will be formed
                // sol that its only the game ID
                settings.OnlyFavorites = false;
                settings.IncludeBroken = false;
                settings.IncludeHidden = false;
                settings.SelectGame = false;
                settings.ShowPlatformsBeforeSelectingGame = false;
                settings.SpecificGameId = game.Id;
                settings.DelayInSeconds = 0;

                try
                {
                    settings.GameState = GameStateEnum.GAME_LAUNCHED;
                    byte[] buffer = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(settings));
                    udpServer?.Send(buffer, buffer.Length, remoteEndpoint);
                }
                catch (Exception ex)
                {
                    LogHelper.LogException(ex, "OnAfterGameLaunched");
                }
            }

            playingGame = true;
        }

        public void OnGameExited()
        {
            BigBoxAutoPlaySettings settings = BigBoxAutoPlaySettingsDataService.Instance.GetSettings();

            // only send if AutoPlay is enabled
            if (settings?.Enabled == true && settings?.ServerEnable == true && settings?.RemoteSync == true)
            {
                try
                {
                    settings.GameState = GameStateEnum.GAME_EXITED;
                    byte[] buffer = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(settings));
                    udpServer?.Send(buffer, buffer.Length, remoteEndpoint);

                }
                catch (Exception ex)
                {
                    LogHelper.LogException(ex, "OnGameExited");
                }
            }
            playingGame = false;
        }

        static void ExitGame()
        {
            const int  VK_ESCAPE                = 0x1B; //ESC key
            const uint KEYEVENTF_KEYDOWN        = 0x0000; // New definition
            const uint KEYEVENTF_KEYUP          = 0x0002;

            try
            {
                //Press the key
                keybd_event((byte)VK_ESCAPE, 0, (int)KEYEVENTF_KEYDOWN | 0, 0);
                keybd_event((byte)VK_ESCAPE, 0, (int)KEYEVENTF_KEYUP | 0, 0);
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex, "ExitGame");

            }
        }
    }
}
