using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public enum ListType
    {
        Income,
        Spending,
        Expenses
    }
    public partial class MainWindow : Window
    {
        public event Action OpenWelcomePage;
        public event Action OpenLoginPage;
        public event Action OpenReimbursementListWindow;
        public event Action OpenChangePasswordWindow;
        public event Action UploadFile;
        public MainWindow()
        {
            InitializeComponent();

            new NavigationService(this);
        }

        public void NavClick(object sender, RoutedEventArgs e)
        {
            TimeSpan duration = TimeSpan.FromSeconds(0.2);
            DoubleAnimation slideAnimation = new DoubleAnimation();
            slideAnimation.Duration = duration;
            slideAnimation.EasingFunction = new QuarticEase();

            if (SideNav.Margin.Left < 0)
            {
                slideAnimation.To = 0;
                BackgroundShade.Visibility = Visibility.Visible;
            }
            else
            {
                slideAnimation.To = -300;
            }

            SideNav.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness((double)slideAnimation.To, 0, 0, 0), duration)); // Apply slide animation to SideNav.Margin

            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                Duration = duration,
                From = BackgroundShade.Opacity,
                To = (slideAnimation.To == -300) ? 0 : 1
            };

            fadeAnimation.Completed += (s, _) =>
            {
                if (slideAnimation.To == -300 && BackgroundShade.Visibility == Visibility.Visible)
                {
                    BackgroundShade.Visibility = Visibility.Collapsed;
                }
            };

            BackgroundShade.BeginAnimation(OpacityProperty, fadeAnimation);
        }

        private void BackgroundClick(object sender, RoutedEventArgs e)
        {
            NavClick(sender, e);
        }

        private void LogOutClick(object sender, RoutedEventArgs e)
        {
            NavClick(sender, e);
            OpenLoginPage?.Invoke();
        }

        private void ClearDataButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to clear all your data?", "Confirm", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                FileManagement.ClearData();
                OpenWelcomePage?.Invoke();
            }
        }

        private void ReimbursementListClick(object sender, RoutedEventArgs e)
        {
            NavClick(sender, e);
            OpenReimbursementListWindow?.Invoke();
        }

        public void UploadFileButtonClick(object sender, RoutedEventArgs e)
        {
            UploadFile?.Invoke();
        }

        private void ChangePassswordClick(object sender, RoutedEventArgs e)
        {
            NavClick(sender, e);
            OpenChangePasswordWindow?.Invoke();
        }
    }
}
