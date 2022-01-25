using BigBoxAutoPlay.AutoPlayers;
using BigBoxAutoPlay.DataAccess;
using BigBoxAutoPlay.Helpers;
using BigBoxAutoPlay.Models;
using System;
using System.ComponentModel;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace BigBoxAutoPlay
{
    public class BigBoxAutoPlay : ISystemEventsPlugin
    {
        public void OnEventRaised(string eventType)
        {
            switch(eventType)
            {
                case SystemEventTypes.BigBoxStartupCompleted:
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += BackgroundWorker_DoWork;
                    backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
                    backgroundWorker.RunWorkerAsync();
                    break;

                default:
                    break;
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                IBigBoxAutoPlayer bigBoxAutoPlayer = BigBoxAutoPlayer.GetBigBoxAutoPlayer();
                bigBoxAutoPlayer.AutoPlay();
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex, "Startup");
            }

        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BigBoxAutoPlaySettings bigBoxAutoPlaySettings = DataService.GetSettings();
                if (bigBoxAutoPlaySettings.DelayInSeconds > 0)
                {
                    System.Threading.Thread.Sleep(bigBoxAutoPlaySettings.DelayInSeconds);
                }
            }
            catch(Exception ex)
            {
                LogHelper.LogException(ex, "Delay");
            }
        }
    }
}
