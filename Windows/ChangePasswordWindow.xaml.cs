using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for ChangePasswordWindow.xaml
    /// </summary>
    public partial class ChangePasswordWindow : Page
    {
        public event Action GoBack;
        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        private void OldPasswordInputGotFocus(object sender, RoutedEventArgs e)
        {
            OldPasswordInput.Text = "";
            OldPasswordInput.Foreground = Brushes.Black;
        }
        private void NewPasswordInputGotFocus(object sender, RoutedEventArgs e)
        {
            NewPasswordInput.Text = "";
            NewPasswordInput.Foreground = Brushes.Black;
        }

        private void OldPasswordInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
            }
        }
        private void NewPasswordInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
            }
        }

        private void BackgroundClick(object sender, MouseButtonEventArgs e)
        {
            CancelButtonClick(sender, e);
        }

        private void ConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(OldPasswordInput.Text) || string.IsNullOrEmpty(NewPasswordInput.Text))
            {
                MessageBox.Show("Please enter all feilds");
                return;
            }
            List<string> profile = FileManagement.ReadProfile();
            string password = profile[0];
            if (OldPasswordInput.Text != password)
            {
                MessageBox.Show("Incorrect password");
                return;
            }
            if (NewPasswordInput.Text == password)
            {
                MessageBox.Show("New password cannot be\nthe same as the old passowrd");
                return;
            }
            if (OldPasswordInput.Text == password && NewPasswordInput.Text != "")
            {
                string tax = profile[1];
                List<string> newProfile = new List<string> { NewPasswordInput.Text, tax };
                FileManagement.WriteProfile(newProfile);
                GoBack?.Invoke();
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            GoBack?.Invoke();
        }
    }
}
