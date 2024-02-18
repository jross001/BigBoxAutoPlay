using BigBoxAutoPlay.DataAccess;
using BigBoxAutoPlay.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace BigBoxAutoPlay.AutoPlayers
{
    public class BigBoxAutoPlayer
    {
        public static void AutoPlay(BigBoxAutoPlaySettings bigBoxAutoPlaySettings)
        {
            autoPlayGame = null;

            // shouldn't be called but check just in case 
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

            if(gamesQuery.Count() > 1)
            {
                Random random = new Random(Guid.NewGuid().GetHashCode());

                int gameCount = gamesQuery.Count();
                int randomIndex = random.Next(0, gamesQuery.Count());
                autoPlayGame = gamesQuery.ElementAt(randomIndex);
            }
            else if(gamesQuery.Count() == 1)
            {
                autoPlayGame = gamesQuery.FirstOrDefault();
            }
            
            if(autoPlayGame != null)
            {
                if(bigBoxAutoPlaySettings.SelectGame.GetValueOrDefault())
                {
                    // filter to find the game 
                    PluginHelper.BigBoxMainViewModel.ShowGame(autoPlayGame, FilterType.None);

                    if (bigBoxAutoPlaySettings.DoNotLaunch == true) return;

                    if (autoPlayGame != null)
                    {
                        // launch the game 
                        PluginHelper.BigBoxMainViewModel.PlayGame(autoPlayGame, null, null, null);
                    }
                }
                else
                {
                    if (bigBoxAutoPlaySettings.DoNotLaunch == true) return;

                    // launch the game 
                    PluginHelper.BigBoxMainViewModel.PlayGame(autoPlayGame, null, null, null);
                }
            }
        }

        private static IGame autoPlayGame = null;
    }
}