using BigBoxAutoPlay.AutoPlayers;
using BigBoxAutoPlay.Helpers;
using System;
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
                    try
                    {
                        IBigBoxAutoPlayer bigBoxAutoPlayer = BigBoxAutoPlayer.GetBigBoxAutoPlayer();
                        bigBoxAutoPlayer.AutoPlay();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogException(ex, "Startup");
                    }                    
                    break;

                default:
                    break;
            }
        }
    }
}
