namespace BigBoxAutoPlay.Models
{
    public class BigBoxAutoPlaySettings
    {
        // enabling functionality
        public bool? Enabled { get; set; }
        public bool? SelectGame { get; set; }
        public bool? LaunchGame { get; set; }
        public int? DelayInSeconds { get; set; }

        // game selection
        public bool? OnlyFavorites { get; set; }
        public bool? IncludeHidden { get; set; }
        public bool? IncludeBroken { get; set; }        
        public string FromPlaylist { get; set; }
        public string FromPlatform { get; set; }
        public string SpecificGameId { get; set; }
        
        // server config
        public bool? CreateServer { get; set; }
        public string ServerIPAddress { get; set; }
        public int? ServerPort { get; set; }        
    }
}
