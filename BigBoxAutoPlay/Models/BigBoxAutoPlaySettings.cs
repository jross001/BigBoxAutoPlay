namespace BigBoxAutoPlay.Models
{
    public class BigBoxAutoPlaySettings
    {
        public string AutoPlayType { get; set; }
        public bool OnlyFavorites { get; set; }
        public string Playlist { get; set; }
        public string Platform { get; set; }
        public string GameTitle { get; set; }
        public int DelayInSeconds { get; set; }
    }
}
