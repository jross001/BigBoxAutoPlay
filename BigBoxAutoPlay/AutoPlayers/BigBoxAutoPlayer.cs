using BigBoxAutoPlay.DataAccess;
using BigBoxAutoPlay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace BigBoxAutoPlay.AutoPlayers
{
    public class BigBoxAutoPlayer
    {
        private readonly BigBoxAutoPlaySettings bigBoxAutoPlaySettings;
        private IGame resolvedGame;

        public static void AutoPlayOnStartup()
        {
            BigBoxAutoPlaySettings settings = BigBoxAutoPlaySettingsDataService.Instance.GetSettings();
            BigBoxAutoPlayer bigBoxAutoPlayer = new BigBoxAutoPlayer(settings);
            bigBoxAutoPlayer.ResolveGame();
            bigBoxAutoPlayer.SelectGame();
            bigBoxAutoPlayer.LaunchGame();            
        }

        public static BigBoxAutoPlayer AutoPlayFromMessage(BigBoxAutoPlaySettings _bigBoxAutoPlaySettings)
        {
            BigBoxAutoPlayer bigBoxAutoPlayer = new BigBoxAutoPlayer(_bigBoxAutoPlaySettings);            
            bigBoxAutoPlayer.ResolveGame();
            bigBoxAutoPlayer.SelectGame();
            bigBoxAutoPlayer.LaunchGame();
            return bigBoxAutoPlayer;
        }
        
        public BigBoxAutoPlayer(BigBoxAutoPlaySettings _bigBoxAutoPlaySettings) 
        {
            bigBoxAutoPlaySettings = _bigBoxAutoPlaySettings;            
        }

        public void ResolveGame()
        {
            resolvedGame = null;

            // bail out if we ain't got no settings
            if (bigBoxAutoPlaySettings == null) return;

            // bail out if settings are disabled 
            if (!bigBoxAutoPlaySettings.Enabled.GetValueOrDefault()) return;

            IEnumerable<IGame> gamesQuery = PluginHelper.DataManager.GetAllGames();

            if (!string.IsNullOrWhiteSpace(bigBoxAutoPlaySettings.SpecificGameId))
            {
                gamesQuery = gamesQuery.Where(g => g.Id == bigBoxAutoPlaySettings.SpecificGameId);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(bigBoxAutoPlaySettings.FromPlaylist))
                {
                    IEnumerable<IPlaylist> allPlaylistsQuery = PluginHelper.DataManager.GetAllPlaylists();
                    IPlaylist playlist = allPlaylistsQuery.FirstOrDefault(p => p.PlaylistId == bigBoxAutoPlaySettings.FromPlaylist);

                    gamesQuery = playlist.GetAllGames(false);
                }

                if (!string.IsNullOrWhiteSpace(bigBoxAutoPlaySettings.FromPlatform))
                {
                    gamesQuery = gamesQuery.Where(g => g.Platform == bigBoxAutoPlaySettings.FromPlatform);
                }

                if (bigBoxAutoPlaySettings.OnlyFavorites.GetValueOrDefault())
                {
                    gamesQuery = gamesQuery.Where(g => g.Favorite);
                }

                if (!bigBoxAutoPlaySettings.IncludeBroken.GetValueOrDefault())
                {
                    gamesQuery = gamesQuery.Where(g => !g.Broken);
                }

                if (!bigBoxAutoPlaySettings.IncludeHidden.GetValueOrDefault())
                {
                    gamesQuery = gamesQuery.Where(g => !g.Hide);
                }
            }

            if (gamesQuery.Count() > 1)
            {
                Random random = new Random(Guid.NewGuid().GetHashCode());

                int gameCount = gamesQuery.Count();
                int randomIndex = random.Next(0, gamesQuery.Count());
                resolvedGame = gamesQuery.ElementAt(randomIndex);
            }
            else if (gamesQuery.Count() == 1)
            {
                resolvedGame = gamesQuery.FirstOrDefault();
            }
        }

        public void SelectGame()
        {
            // bail out if we ain't got no settings
            if (bigBoxAutoPlaySettings == null) return;

            // bail out if settings are disabled 
            if (!bigBoxAutoPlaySettings.Enabled.GetValueOrDefault()) return;

            // bail out if select game is not checked
            if (!bigBoxAutoPlaySettings.SelectGame.GetValueOrDefault()) return;

            // bail out if the game is not resolved 
            if (resolvedGame == null) return;

            // select the game
            PluginHelper.BigBoxMainViewModel.ShowGame(resolvedGame, FilterType.None);
        }

        public void LaunchGame()
        {
            // bail out if we ain't got no settings
            if (bigBoxAutoPlaySettings == null) return;

            // bail out if settings are disabled 
            if (!bigBoxAutoPlaySettings.Enabled.GetValueOrDefault()) return;

            // bail out if launch game is not checked
            if (!bigBoxAutoPlaySettings.LaunchGame.GetValueOrDefault()) return;

            // bail out if the game is not resolved 
            if (resolvedGame == null) return;

            PluginHelper.BigBoxMainViewModel.PlayGame(resolvedGame, null, null, null);
        }
    }
}