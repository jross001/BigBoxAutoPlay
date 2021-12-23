using BigBoxAutoPlay.DataAccess;
using BigBoxAutoPlay.Models;
using System;

namespace BigBoxAutoPlay.AutoPlayers
{
    public abstract class BigBoxAutoPlayer : IBigBoxAutoPlayer
    {
        protected readonly BigBoxAutoPlaySettings bigBoxAutoPlaySettings;
        protected Random random;

        protected BigBoxAutoPlayer(BigBoxAutoPlaySettings _bigBoxAutoPlaySettings)
        {
            bigBoxAutoPlaySettings = _bigBoxAutoPlaySettings;
            random = new Random(Guid.NewGuid().GetHashCode());
        }

        public static IBigBoxAutoPlayer GetBigBoxAutoPlayer()
        {
            IBigBoxAutoPlayer bigBoxAutoPlayer;

            BigBoxAutoPlaySettings bigBoxAutoPlaySettings = DataService.GetSettings();

            switch(bigBoxAutoPlaySettings.BoxAutoPlayType)
            {
                case BigBoxAutoPlayType.RandomGame:
                    bigBoxAutoPlayer = new BigBoxAutoPlayerRandomGame(bigBoxAutoPlaySettings);
                    break;

                case BigBoxAutoPlayType.RandomFavorite:
                    bigBoxAutoPlayer = new BigBoxAutoPlayerRandomFavorite(bigBoxAutoPlaySettings);
                    break;

                case BigBoxAutoPlayType.RandomGameFromPlatform:
                    bigBoxAutoPlayer = new BigBoxAutoPlayerRandomGameFromPlatform(bigBoxAutoPlaySettings);
                    break;

                case BigBoxAutoPlayType.RandomGameFromPlaylist:
                    bigBoxAutoPlayer = new BigBoxAutoPlayerRandomGameFromPlaylist(bigBoxAutoPlaySettings);
                    break;

                case BigBoxAutoPlayType.SpecificGame:
                    bigBoxAutoPlayer = new BigBoxAutoPlayerSpecificGame(bigBoxAutoPlaySettings);
                    break;

                case BigBoxAutoPlayType.Off:
                    bigBoxAutoPlayer = new BigBoxAutoPlayerNone(bigBoxAutoPlaySettings);
                    break;

                default:
                    bigBoxAutoPlayer = new BigBoxAutoPlayerNone(bigBoxAutoPlaySettings);
                    break;
            }

            return bigBoxAutoPlayer;
        }

        public abstract void AutoPlay();
    }
}