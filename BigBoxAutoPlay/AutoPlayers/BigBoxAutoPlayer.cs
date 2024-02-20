using BigBoxAutoPlay.DataAccess;
using BigBoxAutoPlay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace BigBoxAutoPlay.AutoPlayers
{
    public class BigBoxAutoPlayer
    {
        private readonly BigBoxAutoPlaySettings bigBoxAutoPlaySettings;
        private IGame resolvedGame;
        public StringBuilder ResponseStringBuilder { get; set; } = new StringBuilder();

        public static void AutoPlayOnStartup()
        {
            BigBoxAutoPlaySettings settings = BigBoxAutoPlaySettingsDataService.Instance.GetSettings();
            
            BigBoxAutoPlayer bigBoxAutoPlayer = new BigBoxAutoPlayer(settings);
            
            bigBoxAutoPlayer.ResolveGame();
            bigBoxAutoPlayer.SelectGame();
            bigBoxAutoPlayer.LaunchGame();            
        }

        public static string AutoPlayFromMessage(BigBoxAutoPlaySettings _bigBoxAutoPlaySettings)
        {
            BigBoxAutoPlayer bigBoxAutoPlayer = new BigBoxAutoPlayer(_bigBoxAutoPlaySettings);            
            
            bigBoxAutoPlayer.ResolveGame();
            bigBoxAutoPlayer.SelectGame();
            bigBoxAutoPlayer.LaunchGame();

            return bigBoxAutoPlayer.ResponseStringBuilder?.ToString() ?? string.Empty;
        }
        
        public BigBoxAutoPlayer(BigBoxAutoPlaySettings _bigBoxAutoPlaySettings) 
        {
            bigBoxAutoPlaySettings = _bigBoxAutoPlaySettings;            
        }

        public void ResolveGame()
        {
            resolvedGame = null;
            ResponseStringBuilder.AppendLine("Resolving game...");

            // bail out if we ain't got no settings
            if (bigBoxAutoPlaySettings == null)
            {
                ResponseStringBuilder.AppendLine("Settings are null");
                return;
            }


            // bail out if settings are disabled 
            if (!bigBoxAutoPlaySettings.Enabled.GetValueOrDefault())
            {
                ResponseStringBuilder.AppendLine("Auto play is disabled");
                return;
            }

            // change this section up to do the following
            /*
             * 1. Get base query 
             *      - if playlist id specified then from all games in the playlist having that id
             *      - if playlist name specified then from all games in the playlist having that name
             *      - otherwise all games 
             */
            IEnumerable<IGame> baseQuery = null;
            if (!string.IsNullOrWhiteSpace(bigBoxAutoPlaySettings.FromPlaylist))
            {
                ResponseStringBuilder.AppendLine($"Setting up base query from playlist with ID: {bigBoxAutoPlaySettings.FromPlaylist}");

                IPlaylist playlist = PluginHelper.DataManager.GetPlaylistById(bigBoxAutoPlaySettings.FromPlaylist);

                if (playlist == null)
                {
                    ResponseStringBuilder.AppendLine($"Playlist with id {bigBoxAutoPlaySettings.FromPlaylist} was not found");
                    return;
                }

                baseQuery = playlist.GetAllGames(false);
            }
            else if (!string.IsNullOrWhiteSpace(bigBoxAutoPlaySettings.FromPlaylistName))
            {
                ResponseStringBuilder.AppendLine($"Setting up base query from playlist with name: {bigBoxAutoPlaySettings.FromPlaylistName}");

                IPlaylist[] playlists = PluginHelper.DataManager.GetAllPlaylists();
                IPlaylist playlist = playlists?.Where(p => p.Name == bigBoxAutoPlaySettings.FromPlaylistName)?.FirstOrDefault();

                if (playlist == null)
                {
                    ResponseStringBuilder.AppendLine($"Playlist with name {bigBoxAutoPlaySettings.FromPlaylistName} was not found");
                    return;
                }

                baseQuery = playlist.GetAllGames(false);
            }
            else
            {
                ResponseStringBuilder.AppendLine($"Setting up base query from all games");

                baseQuery = PluginHelper.DataManager.GetAllGames();
            }

            if (baseQuery == null || !baseQuery.Any())
            {
                ResponseStringBuilder.AppendLine("No games found in base query");
                return;
            }

            /* 
             * 2. Apply filters
             *      - if platform provided - apply platform filter 
             *      - if favorites checked - apply favorite filter 
             *      - if broken unchecked - apply not broken filter 
             *      - if hidden unchecked - apply not hidden filter 
             *      - if game id specified - apply game id filter 
             *      - if game title specified - apply game title filter 
             */
            IEnumerable<IGame> gamesQuery = baseQuery;
            if (!string.IsNullOrWhiteSpace(bigBoxAutoPlaySettings.FromPlatform))
            {
                ResponseStringBuilder.AppendLine($"Filtering base query with platform: {bigBoxAutoPlaySettings.FromPlatform}");

                gamesQuery = gamesQuery?.Where(g => g.Platform == bigBoxAutoPlaySettings.FromPlatform);

                if (gamesQuery == null || !gamesQuery.Any())
                {
                    ResponseStringBuilder.AppendLine($"No games found with platform {bigBoxAutoPlaySettings.FromPlatform}");
                    return;
                }
            }

            if (bigBoxAutoPlaySettings.OnlyFavorites.GetValueOrDefault())
            {
                ResponseStringBuilder.AppendLine($"Filtering base query with only favorites");

                gamesQuery = gamesQuery?.Where(g => g.Favorite);

                if (gamesQuery == null || !gamesQuery.Any())
                {
                    ResponseStringBuilder.AppendLine($"No games found when including only favorites");
                    return;
                }
            }

            if (!bigBoxAutoPlaySettings.IncludeBroken.GetValueOrDefault())
            {
                ResponseStringBuilder.AppendLine($"Filtering base query to not include broken games");

                gamesQuery = gamesQuery?.Where(g => !g.Broken);

                if (gamesQuery == null || !gamesQuery.Any())
                {
                    ResponseStringBuilder.AppendLine($"No games found when excluding broken games");
                    return;
                }
            }

            if (!bigBoxAutoPlaySettings.IncludeHidden.GetValueOrDefault())
            {
                ResponseStringBuilder.AppendLine($"Filtering base query to not include hidden games");

                gamesQuery = gamesQuery?.Where(g => !g.Hide);

                if (gamesQuery == null || !gamesQuery.Any())
                {
                    ResponseStringBuilder.AppendLine($"No games found when excluding hidden games");
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(bigBoxAutoPlaySettings.GameTitle))
            {
                ResponseStringBuilder.AppendLine($"Filtering base query for game title: {bigBoxAutoPlaySettings.GameTitle}");

                gamesQuery = gamesQuery?.Where(g => g.Title == bigBoxAutoPlaySettings.GameTitle);

                if (gamesQuery == null || !gamesQuery.Any())
                {
                    ResponseStringBuilder.AppendLine($"No games found when filtering for game title: {bigBoxAutoPlaySettings.GameTitle}");
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(bigBoxAutoPlaySettings.SpecificGameId))
            {
                ResponseStringBuilder.AppendLine($"Filtering base query for game ID: {bigBoxAutoPlaySettings.SpecificGameId}");

                gamesQuery = gamesQuery?.Where(g => g.Id == bigBoxAutoPlaySettings.SpecificGameId);

                if (gamesQuery == null || !gamesQuery.Any())
                {
                    ResponseStringBuilder.AppendLine($"No games found when filtering for game ID: {bigBoxAutoPlaySettings.SpecificGameId}");
                    return;
                }
            }

            if (gamesQuery == null || !gamesQuery.Any())
            {
                ResponseStringBuilder.AppendLine("No games found in game query");
                return;
            }

            /* 3.  Finally - pick the game
             *      - if there's one result that's the game
             *      - if there's more than one then pick one at random
             */
            int? gameCount = gamesQuery?.Count();

            ResponseStringBuilder.AppendLine($"Found {gameCount ?? 0} games");

            if (gameCount > 1)
            {
                Random random = new Random(Guid.NewGuid().GetHashCode());
                
                int randomIndex = random.Next(0, gameCount.GetValueOrDefault());

                resolvedGame = gamesQuery?.ElementAt(randomIndex);
            }
            else if (gameCount == 1)
            {
                resolvedGame = gamesQuery?.FirstOrDefault();
            }

            ResponseStringBuilder.AppendLine($"Resolved game: {resolvedGame?.Title ?? "NULL"}");
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

            // show platforms before selecting game so you don't have to escape back through all the selected games to get to the main menu
            if (bigBoxAutoPlaySettings.ShowPlatformsBeforeSelectingGame == true)
            {
                PluginHelper.BigBoxMainViewModel.ShowPlatforms();
            }

            // select the game so marquee will be updated
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

            // launch the game
            PluginHelper.BigBoxMainViewModel.PlayGame(resolvedGame, null, null, null);
        }
    }
}