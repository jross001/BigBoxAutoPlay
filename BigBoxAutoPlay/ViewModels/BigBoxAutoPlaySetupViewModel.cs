using BigBoxAutoPlay.DataProvider;
using BigBoxAutoPlay.Events;
using BigBoxAutoPlay.Helpers;
using BigBoxAutoPlay.Models;
using BigBoxAutoPlay.Resources;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace BigBoxAutoPlay.ViewModels
{
    public class BigBoxAutoPlaySetupViewModel : ViewModelBase
    {
        private IEventAggregator eventAggregator;

        private BigBoxAutoPlaySettingsDataProvider bigBoxAutoPlaySettingsDataProvider;

        private BigBoxAutoPlaySettings bigBoxAutoPlaySettings;

        public ICommand CancelCommand { get; }
        public ICommand OKCommand { get; }
        public ICommand ClearPlatformCommand { get; }
        public ICommand ClearPlaylistCommand { get; }
        public ICommand ClearGameCommand { get; }

        public ObservableCollection<IPlatform> PlatformLookup { get; }
        public ObservableCollection<IPlaylist> PlaylistLookup { get; }
        public ObservableCollection<IGame> GameLookup { get; }

        public BigBoxAutoPlaySetupViewModel()
        {
            eventAggregator = EventAggregatorHelper.Instance.EventAggregator;

            bigBoxAutoPlaySettingsDataProvider = new BigBoxAutoPlaySettingsDataProvider();

            PlatformLookup = new ObservableCollection<IPlatform>();
            PlaylistLookup = new ObservableCollection<IPlaylist>();
            GameLookup = new ObservableCollection<IGame>();

            OKCommand = new DelegateCommand(OnOKExecuteAsync);
            CancelCommand = new DelegateCommand(OnCancelExecute);
            ClearPlatformCommand = new DelegateCommand(OnClearPlatformExecute);
            ClearPlaylistCommand = new DelegateCommand(OnClearPlaylistCommandExecute);
            ClearGameCommand = new DelegateCommand(OnClearGameExecute);
        }

        private void OnClearGameExecute()
        {
            SelectedGame = null;
        }

        private void OnClearPlaylistCommandExecute()
        {
            SelectedPlaylist = null;
        }

        private void OnClearPlatformExecute()
        {
            SelectedPlatform = null;
        }

        private IPlatform selectedPlatform;
        public IPlatform SelectedPlatform
        {
            get => selectedPlatform;
            set
            {
                selectedPlatform = value;
                OnPropertyChanged("SelectedPlatform");

                if (string.IsNullOrWhiteSpace(selectedPlatform?.Name))
                {
                    bigBoxAutoPlaySettings.FromPlatform = string.Empty;
                }
                else
                {
                    bigBoxAutoPlaySettings.FromPlatform = selectedPlatform.Name;
                }

                InitializeGameLookup();
            }
        }

        private IPlaylist selectedPlaylist;
        public IPlaylist SelectedPlaylist
        {
            get => selectedPlaylist;
            set
            {
                selectedPlaylist = value;
                OnPropertyChanged("SelectedPlaylist");

                if (string.IsNullOrWhiteSpace(selectedPlaylist?.PlaylistId))
                {
                    bigBoxAutoPlaySettings.FromPlaylist = string.Empty;
                }
                else
                {
                    bigBoxAutoPlaySettings.FromPlaylist = selectedPlaylist.PlaylistId;                    
                }

                InitializeGameLookup();
            }
        }

        public IGame selectedGame;
        public IGame SelectedGame
        {
            get => selectedGame;
            set
            {
                selectedGame = value;
                OnPropertyChanged("SelectedGame");

                if(string.IsNullOrWhiteSpace(selectedGame?.Id))
                {
                    bigBoxAutoPlaySettings.SpecificGameId = string.Empty;
                }
                else
                {
                    bigBoxAutoPlaySettings.SpecificGameId = selectedGame.Id;
                }
            }
        }



        public async void LoadAsync()
        {
            bigBoxAutoPlaySettings = await bigBoxAutoPlaySettingsDataProvider.GetBigBoxAutoPlaySettings();
            
            InitializePlatformLookup();
            InitializePlaylistLookup();

            InitializeSettings();
        }

        private void InitializeSettings()
        {
            Enabled = bigBoxAutoPlaySettings.Enabled.GetValueOrDefault();
            SelectGame = bigBoxAutoPlaySettings.SelectGame.GetValueOrDefault();
            OnlyFavorites = bigBoxAutoPlaySettings.OnlyFavorites.GetValueOrDefault();
            IncludeHidden = bigBoxAutoPlaySettings.IncludeHidden.GetValueOrDefault();
            IncludeBroken = bigBoxAutoPlaySettings.IncludeBroken.GetValueOrDefault();                                    
            DelayInSeconds = bigBoxAutoPlaySettings.DelayInSeconds.GetValueOrDefault();
            CreateServer = bigBoxAutoPlaySettings.CreateServer.GetValueOrDefault();
            DoNotLaunch = bigBoxAutoPlaySettings.DoNotLaunch.GetValueOrDefault();
            ServerPort = bigBoxAutoPlaySettings.ServerPort.GetValueOrDefault();
            ServerIPAddress = bigBoxAutoPlaySettings.ServerIPAddress;

            if (!string.IsNullOrWhiteSpace(bigBoxAutoPlaySettings.FromPlatform))
            {
                SelectedPlatform = PlatformLookup.FirstOrDefault(p => p.Name == bigBoxAutoPlaySettings.FromPlatform);
            }

            if (!string.IsNullOrWhiteSpace(bigBoxAutoPlaySettings.FromPlaylist))
            {
                SelectedPlaylist = PlaylistLookup.FirstOrDefault(p => p.PlaylistId == bigBoxAutoPlaySettings.FromPlaylist);
            }

            InitializeGameLookup();

            if(!string.IsNullOrWhiteSpace(bigBoxAutoPlaySettings.SpecificGameId))
            {
                SelectedGame = GameLookup.FirstOrDefault(g => g.Id == bigBoxAutoPlaySettings.SpecificGameId);
            }
        }

        private void InitializePlatformLookup()
        {
            IPlatform[] platforms = PluginHelper.DataManager.GetAllPlatforms().OrderBy(p => p.Name).ToArray();
            
            PlatformLookup.Clear();
            foreach (IPlatform platform in platforms)
            {
                PlatformLookup.Add(platform);
            }
        }

        private void InitializePlaylistLookup()
        {
            IPlaylist[] playlists = PluginHelper.DataManager.GetAllPlaylists().OrderBy(p => p.Name).ToArray();

            PlaylistLookup.Clear();
            foreach (IPlaylist playlist in playlists)
            {
                PlaylistLookup.Add(playlist);
            }
        }

        private void InitializeGameLookup()
        {
            IEnumerable<IGame> games = PluginHelper.DataManager.GetAllGames().OrderBy(g => g.SortTitleOrTitle).ToArray();
            
            if (!string.IsNullOrWhiteSpace(SelectedPlaylist?.Name))
            {
                games = SelectedPlaylist.GetAllGames(true);
            }

            if (!string.IsNullOrEmpty(SelectedPlatform?.Name))
            {
                games = games.Where(g => g.Platform == SelectedPlatform.Name);
            }

            GameLookup.Clear();
            foreach (IGame game in games)
            {
                GameLookup.Add(game);
            }
        }

        private void OnCancelExecute()
        {
            eventAggregator.GetEvent<BigBoxAutoPlaySettingsCancelEvent>().Publish();
        }

        private async void OnOKExecuteAsync()
        {
            await bigBoxAutoPlaySettingsDataProvider.SaveBigBoxAutoPlaySettingsAsync(bigBoxAutoPlaySettings);
            eventAggregator.GetEvent<BigBoxAutoPlaySettingsOKEvent>().Publish();
        }

        public Uri IconUri { get; } = ResourceImages.RomHackingIconPath;

        public bool Enabled
        {
            get => bigBoxAutoPlaySettings?.Enabled == true;
            set
            {
                bigBoxAutoPlaySettings.Enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        public bool SelectGame
        {
            get => bigBoxAutoPlaySettings?.SelectGame == true;
            set
            {
                bigBoxAutoPlaySettings.SelectGame = value;
                OnPropertyChanged("SelectGame");
            }
        }

        public bool OnlyFavorites
        {
            get => bigBoxAutoPlaySettings?.OnlyFavorites == true;
            set
            {
                bigBoxAutoPlaySettings.OnlyFavorites = value;
                OnPropertyChanged("OnlyFavorites");
            }
        }

        public bool IncludeHidden
        {
            get => bigBoxAutoPlaySettings?.IncludeHidden == true;
            set
            {
                bigBoxAutoPlaySettings.IncludeHidden = value;
                OnPropertyChanged("IncludeHidden");
            }
        }

        public bool IncludeBroken
        {
            get => bigBoxAutoPlaySettings?.IncludeBroken == true;
            set
            {
                bigBoxAutoPlaySettings.IncludeBroken = value;
                OnPropertyChanged("IncludeBroken");
            }
        }

        public int DelayInSeconds
        {
            get => bigBoxAutoPlaySettings?.DelayInSeconds ?? 0;
            set
            {
                bigBoxAutoPlaySettings.DelayInSeconds = value;
                OnPropertyChanged("DelayInSeconds");
            }
        }

        public bool CreateServer
        {
            get => bigBoxAutoPlaySettings?.CreateServer == true;
            set
            {
                bigBoxAutoPlaySettings.CreateServer = value;
                OnPropertyChanged("CreateServer");
            }
        }

        public bool DoNotLaunch
        {
            get => bigBoxAutoPlaySettings?.DoNotLaunch == true;
            set
            {
                bigBoxAutoPlaySettings.DoNotLaunch = value;
                OnPropertyChanged("DoNotLaunch");
            }
        }

        public int ServerPort
        {
            get => bigBoxAutoPlaySettings?.ServerPort ?? 0;
            set
            {
                bigBoxAutoPlaySettings.ServerPort = value;
                OnPropertyChanged("ServerPort");
            }
        }

        public string ServerIPAddress
        {
            get => bigBoxAutoPlaySettings?.ServerIPAddress;
            set
            {
                bigBoxAutoPlaySettings.ServerIPAddress = value;
                OnPropertyChanged("ServerIPAddress");
            }
        }
    }
}
