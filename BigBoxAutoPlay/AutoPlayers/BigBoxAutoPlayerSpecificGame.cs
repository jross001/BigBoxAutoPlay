using BigBoxAutoPlay.Models;
using System.Collections.Generic;
using System.Linq;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace BigBoxAutoPlay.AutoPlayers
{
    public class BigBoxAutoPlayerSpecificGame : BigBoxAutoPlayer
    {
        public BigBoxAutoPlayerSpecificGame(BigBoxAutoPlaySettings _bigBoxAutoPlaySettings) : base(_bigBoxAutoPlaySettings)
        {
        }

        public override void AutoPlay()
        {
            IEnumerable<IGame> gamesQuery = PluginHelper.DataManager.GetAllGames();

            IGame game = gamesQuery.FirstOrDefault(p =>
                p.Platform == bigBoxAutoPlaySettings.Platform &&
                p.Title == bigBoxAutoPlaySettings.GameTitle);

            if(game == null)
            {
                return;
            }

            game.Play();
        }
    }
}