using System;
using System.Windows;
using System.Windows.Controls;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for WelcomePage.xaml
    /// </summary>
    public partial class WelcomePage : Page
    {
        public event Action UploadFile;
        public WelcomePage()
        {
            InitializeComponent();
        }

        private void UploadFileButtonClick(object sender, RoutedEventArgs e)
        {
            UploadFile?.Invoke();
        }
    }
}
