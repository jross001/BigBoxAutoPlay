using BigBoxAutoPlay.Helpers;
using BigBoxAutoPlay.Models;
using Newtonsoft.Json;
using System.IO;

namespace BigBoxAutoPlay.DataAccess
{
    public class DataService
    {
        private string StorageFile = DirectoryInfoHelper.Instance.SettingsFile;

        public static BigBoxAutoPlaySettings GetSettings()
        {
            return Instance.ReadSettingsFile();
        }

        private BigBoxAutoPlaySettings ReadSettingsFile()
        {
            // make sure the data file exists 
            if (!File.Exists(StorageFile))
            {
                // make sure the folders exist 
                DirectoryInfoHelper.CreateFolders();

                BigBoxAutoPlaySettings bigBoxAutoPlaySettings = new BigBoxAutoPlaySettings()
                {
                    BoxAutoPlayType = BigBoxAutoPlayType.RandomGame,
                    OnlyFavorites = false,
                    Platform = "",
                    Playlist = "",
                    GameTitle = ""
                };

                // save the file 
                SaveToFileAsync(bigBoxAutoPlaySettings);

                return bigBoxAutoPlaySettings;
            }

            // read and deserialize the file
            string json = File.ReadAllText(StorageFile);
            return JsonConvert.DeserializeObject<BigBoxAutoPlaySettings>(json);
        }

        private void SaveToFileAsync(BigBoxAutoPlaySettings bigBoxAutoPlaySettings)
        {
            string json = JsonConvert.SerializeObject(bigBoxAutoPlaySettings, Formatting.Indented);
            File.WriteAllText(StorageFile, json);
        }

        #region singleton implementation 
        public static DataService Instance
        {
            get
            {
                return instance;
            }
        }

        private static readonly DataService instance = new DataService();

        static DataService()
        {
        }

        private DataService()
        {
        }
        #endregion
    }
}
