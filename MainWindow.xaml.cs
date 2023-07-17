using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

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

            _ = new NavigationService(this);
        }

        public void NavigateToPage(Page content)
        {
            MainFrame.Content = content;
        }

        public async void OpenPopup2(Page content)
        {
            Popup2.Content = content;
            await WindowFade(Popup2, true);
        }

        public async void OpenPopup1(Page content)
        {
            Popup1.Content = content;
            await WindowFade(Popup1, true);
        }

        public async void ClosePopup2()
        {
            await WindowFade(Popup2, false);
            Popup2.Content = null;     
        }
        public async void ClosePopups()
        {
            await WindowFade(Popup1, false);
            Popup1.Content = null;
            await WindowFade(Popup2, false);
            Popup1.Content = null;
        }

        private async Task WindowFade(Frame frame, bool isOpening)
        {
            if (isOpening)
            {
                frame.Opacity = 0;
                frame.Visibility = Visibility.Visible;
            }

            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.15),
                From = isOpening ? 0 : 1,
                To = isOpening ? 1 : 0,
            };

            var tcs = new TaskCompletionSource<bool>();

            fadeAnimation.Completed += (s, _) => tcs.SetResult(true);

            frame.BeginAnimation(OpacityProperty, fadeAnimation);

            await tcs.Task;

            if (!isOpening)
            {
                frame.Visibility = Visibility.Collapsed;
            }
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
