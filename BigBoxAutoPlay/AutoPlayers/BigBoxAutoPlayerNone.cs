using BigBoxAutoPlay.Models;

namespace BigBoxAutoPlay.AutoPlayers
{
    public class BigBoxAutoPlayerNone : BigBoxAutoPlayer
    {
        public BigBoxAutoPlayerNone(BigBoxAutoPlaySettings _bigBoxAutoPlaySettings) : base(_bigBoxAutoPlaySettings)
        {
        }

        public override void AutoPlay()
        {
            // intentionally left blank - none type does nothing 
        }
    }
}