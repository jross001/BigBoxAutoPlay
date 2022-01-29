using BigBoxAutoPlay.Events;
using BigBoxAutoPlay.Helpers;
using BigBoxAutoPlay.ViewModels;
using Prism.Events;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace BigBoxAutoPlay.Views
{
    /// <summary>
    /// Interaction logic for BigBoxAutoPlaySetupView.xaml
    /// </summary>
    public partial class BigBoxAutoPlaySetupView : Window
    {
        private readonly EventAggregator eventAggregator;
        private readonly BigBoxAutoPlaySetupViewModel bigBoxAutoPlaySetupViewModel;

        public BigBoxAutoPlaySetupView(BigBoxAutoPlaySetupViewModel _bigBoxAutoPlaySetupViewModel)
        {
            InitializeComponent();

            eventAggregator = EventAggregatorHelper.Instance.EventAggregator;
            eventAggregator.GetEvent<BigBoxAutoPlaySettingsCancelEvent>().Subscribe(OnCancel);
            eventAggregator.GetEvent<BigBoxAutoPlaySettingsOKEvent>().Subscribe(OnOK);

            bigBoxAutoPlaySetupViewModel = _bigBoxAutoPlaySetupViewModel;
            DataContext = bigBoxAutoPlaySetupViewModel;
            Loaded += BigBoxAutoPlaySetup_Loaded;
            PreviewKeyDown += BigBoxAutoPlaySetup_PreviewKeyDown;
        }

        private void BigBoxAutoPlaySetup_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void BigBoxAutoPlaySetup_Loaded(object sender, RoutedEventArgs e)
        {
            bigBoxAutoPlaySetupViewModel.LoadAsync();
        }

        private void OnCancel()
        {
            Close();
        }

        private void OnOK()
        {
            Close();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(e.OriginalSource is Grid)
            {
                WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
