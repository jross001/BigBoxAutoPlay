namespace BigBoxAutoPlay.Models
{
    // Need until BigBox API provides an interface
    public enum GameStateEnum { IDLE, GAME_LAUNCHED, GAME_EXITED }

    public class BigBoxAutoPlaySettings
    {
        // enabling functionality
        public bool? Enabled { get; set; }
        public bool? SelectGame { get; set; }
        public bool? ShowPlatformsBeforeSelectingGame { get; set; }
        public bool? LaunchGame { get; set; }
        public int? DelayInSeconds { get; set; }

        // game selection
        public bool? OnlyFavorites { get; set; }
        public bool? IncludeHidden { get; set; }
        public bool? IncludeBroken { get; set; }        
        public string FromPlaylist { get; set; }
        public string FromPlatform { get; set; }
        public string SpecificGameId { get; set; }
        
        // UDP server config
        public bool? ServerEnable { get; set; }
        public int? ServerPort { get; set; }

        // UDP Remote Client config
        public bool? RemoteSync { get; set; }
        public string RemoteIPAddress { get; set; }
        public int? RemotePort { get; set; }

        // UDP Multicast config
        public bool? MulticastEnable { get; set; }
        public string MulticastAddress { get; set; }

        // additional parameters for launching game from UDP server
        public string FromPlaylistName { get; set; }
        public string GameTitle { get; set; }       
        public GameStateEnum? GameState { get; set; }
    }
}
