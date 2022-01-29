using BigBoxAutoPlay.ViewModels;
using BigBoxAutoPlay.Views;
using Unbroken.LaunchBox.Plugins;

namespace BigBoxAutoPlay
{
    public class BigBoxAutoPlaySetup : ISystemMenuItemPlugin
    {
        public string Caption => "Configure BigBox AutoPlay";

        public System.Drawing.Image IconImage => Properties.Resources.AutoPlayIcon;

        public bool ShowInLaunchBox => true;

        public bool ShowInBigBox => false;

        public bool AllowInBigBoxWhenLocked => false;

        public void OnSelected()
        {
            BigBoxAutoPlaySetupViewModel bigBoxAutoPlaySetupViewModel = new BigBoxAutoPlaySetupViewModel();
            BigBoxAutoPlaySetupView bigBoxAutoPlaySetupView = new BigBoxAutoPlaySetupView(bigBoxAutoPlaySetupViewModel);
            bigBoxAutoPlaySetupView.Show();
        }
    }
}
