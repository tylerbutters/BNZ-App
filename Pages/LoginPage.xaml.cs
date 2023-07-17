using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;
using System.Windows.Media;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public event Action OpenHomepage;
        private int attempts = 3;
        public LoginPage()
        {
            InitializeComponent();

            PasswordInput.Focus();
        }

        private void AuthenticateAccount()
        {
            List<string> profile = FileManagement.ReadProfile();
            string password = profile[0];

            if (PasswordInput.Text == password)
            {
                OpenHomepage?.Invoke();
            }
            else
            {
                attempts--;
                if (attempts == 0)
                {
                    MessageBox.Show("You've run out of attempts.\nShutting down application...");
                    Application.Current.Shutdown();
                }
                MessageBox.Show("Incorrect passowrd\n" + attempts + " attempt(s) remaining");
                PasswordInput.Text = "";
            }
        }

        private void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            AuthenticateAccount();
        }

        private void PasswordInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AuthenticateAccount();
            }
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
