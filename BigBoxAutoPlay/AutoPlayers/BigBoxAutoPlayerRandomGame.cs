using BigBoxAutoPlay.Models;
using System.Collections.Generic;
using System.Linq;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace BigBoxAutoPlay.AutoPlayers
{
    public class BigBoxAutoPlayerRandomGame : BigBoxAutoPlayer
    {
        public BigBoxAutoPlayerRandomGame(BigBoxAutoPlaySettings _bigBoxAutoPlaySettings) : base(_bigBoxAutoPlaySettings)
        {

        }

        public override void AutoPlay()
        {
            IEnumerable<IGame> gamesQuery = PluginHelper.DataManager.GetAllGames().Where(g => !g.Broken && !g.Hide);

            if (bigBoxAutoPlaySettings.OnlyFavorites)
            {
                gamesQuery = gamesQuery.Where(g => g.Favorite);
            }

            if(!gamesQuery.Any())
            {
                return;
            }

            int gameCount = gamesQuery.Count();
            int randomIndex = random.Next(0, gamesQuery.Count());
            IGame randomGame = gamesQuery.ElementAt(randomIndex);

            if(randomGame == null)
            {
                return;
            }

            randomGame.Play();
        }
    }
}