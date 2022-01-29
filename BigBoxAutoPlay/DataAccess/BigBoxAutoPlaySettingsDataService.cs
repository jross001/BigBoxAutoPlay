using BigBoxAutoPlay.Helpers;
using BigBoxAutoPlay.Models;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace BigBoxAutoPlay.DataAccess
{
    public class BigBoxAutoPlaySettingsDataService
    {
        private string StorageFile = DirectoryInfoHelper.Instance.SettingsFile;

        public async Task<BigBoxAutoPlaySettings> GetSettingsAsync()
        {
            return await Instance.ReadSettingsFileAsync();
        }

        public BigBoxAutoPlaySettings GetSettings()
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

                // default values 
                BigBoxAutoPlaySettings bigBoxAutoPlaySettings = new BigBoxAutoPlaySettings()
                {
                    Enabled = false,
                    OnlyFavorites = false,
                    IncludeHidden = false,
                    IncludeBroken = false,
                    FromPlaylist = string.Empty,
                    FromPlatform = string.Empty,
                    SpecificGameId = string.Empty,
                    DelayInSeconds = 2
                };

                // save the file 
                SaveBigBoxAutoPlaySettings(bigBoxAutoPlaySettings);
                return bigBoxAutoPlaySettings;
            }

            // read and deserialize the file
            string json = File.ReadAllText(StorageFile);
            return JsonConvert.DeserializeObject<BigBoxAutoPlaySettings>(json);
        }

        private async Task<BigBoxAutoPlaySettings> ReadSettingsFileAsync()
        {
            // make sure the data file exists 
            if (!File.Exists(StorageFile))
            {
                // make sure the folders exist 
                DirectoryInfoHelper.CreateFolders();

                // default values 
                BigBoxAutoPlaySettings bigBoxAutoPlaySettings = new BigBoxAutoPlaySettings()
                {
                    Enabled = false,
                    OnlyFavorites = false,
                    IncludeHidden = false,
                    IncludeBroken = false,
                    FromPlaylist = string.Empty,
                    FromPlatform = string.Empty,
                    SpecificGameId = string.Empty,
                    DelayInSeconds = 2
                };

                // save the file 
                await SaveBigBoxAutoPlaySettingsAsync(bigBoxAutoPlaySettings);
                return bigBoxAutoPlaySettings;
            }

            // read and deserialize the file
            return await Task.Run(() =>
            {
                string json = File.ReadAllText(StorageFile);
                return JsonConvert.DeserializeObject<BigBoxAutoPlaySettings>(json);
            });
        }

        public async Task SaveBigBoxAutoPlaySettingsAsync(BigBoxAutoPlaySettings bigBoxAutoPlaySettings)
        {
            await Task.Run(() =>
            {
                string json = JsonConvert.SerializeObject(bigBoxAutoPlaySettings, Formatting.Indented);
                File.WriteAllText(StorageFile, json);
            });
        }

        public void SaveBigBoxAutoPlaySettings(BigBoxAutoPlaySettings bigBoxAutoPlaySettings)
        {
            string json = JsonConvert.SerializeObject(bigBoxAutoPlaySettings, Formatting.Indented);
            File.WriteAllText(StorageFile, json);
        }

        #region singleton implementation 
        public static BigBoxAutoPlaySettingsDataService Instance
        {
            get
            {
                return instance;
            }
        }

        private static readonly BigBoxAutoPlaySettingsDataService instance = new BigBoxAutoPlaySettingsDataService();

        static BigBoxAutoPlaySettingsDataService()
        {
        }

        private BigBoxAutoPlaySettingsDataService()
        {
        }
        #endregion
    }
}
