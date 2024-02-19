using BigBoxAutoPlay.AutoPlayers;
using BigBoxAutoPlay.DataAccess;
using BigBoxAutoPlay.Helpers;
using BigBoxAutoPlay.Models;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace BigBoxAutoPlay
{
    public class BigBoxAutoPlayPlugin : ISystemEventsPlugin
    {        
        private static Thread listenerThread;
        private static TcpListener tcpListener;
        private static Dispatcher dispatcher;

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
                tcpListener?.Server?.Close();
            }
            catch(Exception ex)
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
            try
            {
                BigBoxAutoPlaySettings bigBoxAutoPlaySettings = BigBoxAutoPlaySettingsDataService.Instance.GetSettings();

                if (bigBoxAutoPlaySettings?.CreateServer == true)
                {
                    IPAddress ipAddress = IPAddress.Parse(bigBoxAutoPlaySettings.ServerIPAddress);
                    int port = bigBoxAutoPlaySettings.ServerPort.GetValueOrDefault();

                    // listenerThread = new Thread(() => StartListener(ipAddress, port, dispatcher));
                    listenerThread = new Thread(() => StartListener(ipAddress, port));
                    listenerThread.Start();
                }
            }
            catch(Exception ex)
            {
                LogHelper.LogException(ex, "CreateServer");
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
        static void StartListener(IPAddress ipAddress, int port)
        {
            try
            {
                tcpListener = new TcpListener(ipAddress, port);
                tcpListener.Start();

                while (true)
                {
                    // Accept the pending client connection
                    TcpClient client = tcpListener.AcceptTcpClient();
                    
                    // Get the network stream for sending and receiving data
                    NetworkStream stream = client.GetStream();

                    // Read data from the client
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    BigBoxAutoPlaySettings bigBoxAutoPlaySettings = null;
                    try
                    {
                        bigBoxAutoPlaySettings = JsonConvert.DeserializeObject<BigBoxAutoPlaySettings>(receivedData);
                    }
                    catch (Exception ex)
                    {
                        SendResponse(stream, $"Error attempting to convert message to bigBoxAutoPlaySettings. {ex.Message}");
                        LogHelper.LogException(ex, $"Attempting to convert message to bigBoxAutoPlaySettings\n{receivedData}");
                    }

                    if (bigBoxAutoPlaySettings != null)
                    {
                        SendResponse(stream, $"Received message: {receivedData}");

                        dispatcher.Invoke(() =>
                        {
                            BigBoxAutoPlayer.AutoPlayFromMessage(bigBoxAutoPlaySettings);
                        });
                    }

                    // Close the connection
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex, "StartListener");
            }
            finally
            {
                tcpListener.Stop();
            }
        }

        static void SendResponse(NetworkStream stream, string response)
        {
            try
            {
                string responseData = "Hello from the server!";
                byte[] responseBuffer = Encoding.ASCII.GetBytes(responseData);
                stream.Write(responseBuffer, 0, responseBuffer.Length);
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex, "SendResponse");
            }
        }
    }
}
