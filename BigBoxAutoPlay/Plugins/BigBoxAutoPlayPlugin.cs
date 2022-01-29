using BigBoxAutoPlay.AutoPlayers;
using BigBoxAutoPlay.DataAccess;
using BigBoxAutoPlay.Helpers;
using BigBoxAutoPlay.Models;
using System;
using System.ComponentModel;
using System.Threading;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace BigBoxAutoPlay
{
    public class BigBoxAutoPlayPlugin : ISystemEventsPlugin
    {
        BigBoxAutoPlaySettings bigBoxAutoPlaySettings;

        public void OnEventRaised(string eventType)
        {
            try
            {
                switch (eventType)
                {
                    case SystemEventTypes.BigBoxStartupCompleted:
                        BackgroundWorker backgroundWorker = new BackgroundWorker();
                        backgroundWorker.DoWork += DoBackgroundDelay;
                        backgroundWorker.RunWorkerCompleted += DoAutoPlay;
                        backgroundWorker.RunWorkerAsync();                        
                        break;

                    default:
                        break;
                }
            }
            catch(Exception ex)
            {
                LogHelper.LogException(ex, "OnEventRaised");
            }
        }

        private void DoBackgroundDelay(object sender, DoWorkEventArgs e)
        {
            try
            {
                bigBoxAutoPlaySettings = BigBoxAutoPlaySettingsDataService.Instance.GetSettings();
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


        private void DoAutoPlay(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (bigBoxAutoPlaySettings?.Enabled == true)
                {
                    BigBoxAutoPlayer.AutoPlay(bigBoxAutoPlaySettings);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex, "DoAutoPlay");
            }

        }

    }
}
