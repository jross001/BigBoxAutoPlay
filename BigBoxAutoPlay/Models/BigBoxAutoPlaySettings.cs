namespace BigBoxAutoPlay.Models
{
    public class BigBoxAutoPlaySettings
    {
        public bool? Enabled { get; set; }
        public bool? SelectGame { get; set; }
        public bool? OnlyFavorites { get; set; }
        public bool? IncludeHidden { get; set; }
        public bool? IncludeBroken { get; set; }        
        public string FromPlaylist { get; set; }
        public string FromPlatform { get; set; }
        public string SpecificGameId { get; set; }
        public int? DelayInSeconds { get; set; }
        public bool? CreateServer { get; set; }
        public string ServerIPAddress { get; set; }
        public int? ServerPort { get; set; }
        public bool? DoNotLaunch { get; set; }
    }
}
