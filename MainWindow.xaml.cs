using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private Homepage homepage;
        public MainWindow()
        {
            InitializeComponent();

            CreateHomepage();
        }

        private async void OpenEditTransactionWindow(object sender, Transaction transaction1, Transaction transaction2)
        {
            Popup2.Content = null;
            EditTransactionWindow editTransactionWindow = new EditTransactionWindow(transaction1, transaction2);
            Popup1.Content = editTransactionWindow;
            await WindowFade(Popup1, true);

            editTransactionWindow.GoBack += BackToHomepage;
            editTransactionWindow.OpenAddReimbursementWindow += OpenAddReimbursementWindow;
            editTransactionWindow.OpenRemoveReimbursementWindow += OpenRemoveReimbursementWindow;
            editTransactionWindow.ReturnTransaction += ReturnTransaction;
        }

        private async void OpenListWindow(object sender, List<ListItem> list, ListType type)
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            ListWindow listWindow = new ListWindow(list, type);
            
            Popup1.Content = listWindow;
            await WindowFade(Popup1, true);

            listWindow.GoBack += BackToHomepage;
        }

        private async void OpenAddReimbursementWindow(object sender, Transaction transaction1, Transaction transaction2)
        {
            if (transaction1 is null)
            {
                throw new ArgumentNullException(nameof(transaction1));
            }

            if (transaction2 is null)
            {
                throw new ArgumentNullException(nameof(transaction2));
            }

            AddReimbursementWindow addReimbursementWindow = new AddReimbursementWindow(transaction1, transaction2);
            Popup2.Content = addReimbursementWindow;
            await WindowFade(Popup2, true);

            addReimbursementWindow.GoBack += OpenEditTransactionWindow;
            addReimbursementWindow.GoBackHome += BackToHomepage;
        }

        private async void OpenRemoveReimbursementWindow(object sender, Transaction transaction)
        {
            if (transaction is null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            RemoveReimbursementWindow removeReimbursementWindow = new RemoveReimbursementWindow(transaction);
            Popup2.Content = removeReimbursementWindow;
            await WindowFade(Popup2, true);

            removeReimbursementWindow.GoBack += OpenEditTransactionWindow;
            removeReimbursementWindow.GoBackHome += BackToHomepage;
        }

        private async void BackToHomepage(object sender, bool reloadPage)
        {
            if (reloadPage)
            {
                homepage.LoadPage();
            }
            await WindowFade(Popup1, false);
            Popup1.Content = null;
            await WindowFade(Popup2, false);
            Popup2.Content = null;       
        }

        private void ReturnTransaction(object sender, Transaction transaction)
        {
            homepage.transactionGridPage.ReturnTransaction(transaction);
        }

        private void ClearData()
        {
            FileManagement.ClearData();
            CreateHomepage();
        }

        private void CreateHomepage()
        {
            homepage = new Homepage();
            MainFrame.Content = homepage;
            Popup1.Content = null;
            homepage.OpenListWindow += OpenListWindow;
            homepage.transactionGridPage.OpenEditTransactionWindow += OpenEditTransactionWindow;
        }

        private async Task WindowFade(System.Windows.Controls.Frame frame, bool isOpening)
        {
            if (isOpening)
            {
                frame.Opacity = 0;
                frame.Visibility = Visibility.Visible;
            }

            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.1),
                From = isOpening ? 0 : 1,
                To = isOpening ? 1 : 0,
            };

            var tcs = new TaskCompletionSource<bool>();

            fadeAnimation.Completed += (s, _) => tcs.SetResult(true);

            frame.BeginAnimation(Frame.OpacityProperty, fadeAnimation);

            await tcs.Task;

            if (!isOpening)
            {
                frame.Visibility = Visibility.Collapsed;
            }
        }

        private void NavClick(object sender, RoutedEventArgs e)
        {
            TimeSpan duration = TimeSpan.FromSeconds(0.2);
            DoubleAnimation slideAnimation = new DoubleAnimation();
            slideAnimation.Duration = duration;
            slideAnimation.EasingFunction = new CubicEase();

            if (SideNav.Margin.Left < 0)
            {
                slideAnimation.To = 0;
                BackgroundShade.Visibility = Visibility.Visible;
            }
            else
            {
                slideAnimation.To = -300;
            }

            SideNav.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness((float)slideAnimation.To, 0, 0, 0), duration)); // Apply slide animation to SideNav.Margin

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

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ClearDataButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to clear all your data?", "Confirm", MessageBoxButton.YesNo);

            if (result is MessageBoxResult.Yes)
            {
                ClearData();
            }
        }

        private void ReimbursementListClick(object sender, RoutedEventArgs e)
        {
            NavClick(sender, e);
            ReimbursementListWindow reimbursementListWindow = new ReimbursementListWindow();
            Popup1.Content = reimbursementListWindow;

            reimbursementListWindow.GoBack += BackToHomepage;
        }

        private void UploadFileButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Select a CSV file"
            };

            if (openFileDialog.ShowDialog() is true) //if user selected a file
            {
                string selectedFilePath = openFileDialog.FileName;
                List<Transaction> newTransactions = FileManagement.ReadNewFile(selectedFilePath);
                if (newTransactions is null)
                {
                    return;
                }

                File.Copy(selectedFilePath, FileManagement.TransactionsFile, true);
                FileManagement.WriteTransactions(newTransactions);

                CreateHomepage();
            }
        }
    }
}
