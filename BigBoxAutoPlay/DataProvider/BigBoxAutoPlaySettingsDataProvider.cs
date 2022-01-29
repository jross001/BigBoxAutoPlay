using BigBoxAutoPlay.Models;
using BigBoxAutoPlay.DataAccess;
using System.Threading.Tasks;

namespace BigBoxAutoPlay.DataProvider
{
    public class BigBoxAutoPlaySettingsDataProvider
    {
        public BigBoxAutoPlaySettingsDataProvider()
        {
        }

        public async Task<BigBoxAutoPlaySettings> GetBigBoxAutoPlaySettings()
        {
            return await BigBoxAutoPlaySettingsDataService.Instance.GetSettingsAsync();
        }

        public async Task SaveBigBoxAutoPlaySettingsAsync(BigBoxAutoPlaySettings bigBoxAutoPlaySettings)
        {
            await BigBoxAutoPlaySettingsDataService.Instance.SaveBigBoxAutoPlaySettingsAsync(bigBoxAutoPlaySettings);
        }
    }
}
