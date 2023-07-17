using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace BNZApp
{
    public class NavigationService
    {
        private MainWindow mainWindow;
        private Homepage homepage;
        private EditItemWindow editItemWindow;
        private ListWindow listWindow;

        public NavigationService(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            mainWindow.SideNav.Visibility = Visibility.Collapsed;
            mainWindow.OpenWelcomePage += OpenWelcomePage;
            mainWindow.OpenReimbursementListWindow += OpenReimbursementListWindow;
            mainWindow.UploadFile += UploadFile;
            mainWindow.OpenLoginPage += OpenLoginPage;
            mainWindow.OpenChangePasswordWindow += OpenChangePasswordWindow;

            if (FileManagement.ReadTransactions() is null)
            {
                OpenWelcomePage();
            }
            else
            {
                LoginPage loginPage = new LoginPage();                
                loginPage.OpenHomepage += CreateHomepage;
                NavigateToPage(loginPage);
            }
        }

        private void CreateHomepage()
        {
            homepage = new Homepage();
            homepage.OpenListWindow += OpenListWindow;
            homepage.TransactionGridPage.OpenEditTransactionWindow += OpenEditTransactionWindow;
            mainWindow.SideNav.Visibility = Visibility.Visible;         
            NavigateToPage(homepage);
        }

        private void OpenLoginPage()
        {
            LoginPage loginPage = new LoginPage();
            mainWindow.SideNav.Visibility = Visibility.Collapsed;
            NavigateToPage(loginPage);
        }
        public void OpenWelcomePage()
        {
            WelcomePage welcomePage = new WelcomePage();
            welcomePage.UploadFile += UploadFile;
            NavigateToPage(welcomePage);
        }

        private void UploadFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Select a CSV file"
            };

            if (openFileDialog.ShowDialog() == true) // If user selected a file
            {
                string selectedFilePath = openFileDialog.FileName;
                List<Transaction> newTransactions = FileManagement.ReadNewFile(selectedFilePath);
                if (newTransactions is null)
                {
                    MessageBox.Show("Invalid file\nPlease try again");
                    return;
                }
                MessageBox.Show("Upload Successful!");
                FileManagement.WriteTransactions(newTransactions);
                CreateHomepage();
            }
        }

        private void OpenChangePasswordWindow()
        {
            ChangePasswordWindow changePasswordWindow = new ChangePasswordWindow();
            changePasswordWindow.GoBack += () => ClosePopups();
            OpenPopup1(changePasswordWindow);
        }

        private void OpenEditTransactionWindow(Transaction transaction1, Transaction transaction2)
        {
            EditTransactionWindow editTransactionWindow = new EditTransactionWindow(transaction1, transaction2);
            editTransactionWindow.GoBack += BackToHomepage;
            editTransactionWindow.OpenAddReimbursementWindow += OpenAddReimbursementWindow;
            editTransactionWindow.OpenRemoveReimbursementWindow += OpenRemoveReimbursementWindow;
            editTransactionWindow.ReturnTransaction += ReturnTransaction;
            OpenPopup1(editTransactionWindow);
        }

        private void OpenEditItemWindow(ListItem oldItem)
        {
            if (oldItem is null)
            {
                throw new ArgumentNullException(nameof(oldItem));
            }

            editItemWindow = new EditItemWindow(oldItem);
            editItemWindow.GoBack += (_oldItem, _newItem) =>
            {
                listWindow.CloseEditItemWindow(_oldItem, _newItem);
                ClosePopup2();
            };
            OpenPopup2(editItemWindow);
        }

        private void OpenListWindow(List<ListItem> list, ListType type)
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            listWindow = new ListWindow(list, type);
            listWindow.OpenEditItemWindow += OpenEditItemWindow;
            listWindow.GoBack += BackToHomepage;
            OpenPopup1(listWindow);
        }

        private void OpenAddReimbursementWindow(Transaction transaction1, Transaction transaction2)
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
            addReimbursementWindow.GoBack += () => ClosePopup2();
            addReimbursementWindow.GoBackHome += BackToHomepage;
            OpenPopup2(addReimbursementWindow);
        }

        private void OpenRemoveReimbursementWindow(Transaction transaction)
        {
            if (transaction is null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            RemoveReimbursementWindow removeReimbursementWindow = new RemoveReimbursementWindow(transaction);
            removeReimbursementWindow.GoBack += () => ClosePopup2();
            removeReimbursementWindow.GoBackHome += BackToHomepage;
            OpenPopup2(removeReimbursementWindow);
        }

        private void OpenReimbursementListWindow()
        {
            ReimbursementListWindow reimbursementListWindow = new ReimbursementListWindow();
            reimbursementListWindow.GoBack += BackToHomepage;
            OpenPopup1(reimbursementListWindow);
        }

        private void BackToHomepage(bool reloadPage)
        {
            if (reloadPage)
            {
                homepage.LoadPage();
            }
            ClosePopups();
        }

        private void ReturnTransaction(Transaction transaction)
        {
            homepage.TransactionGridPage.ReturnTransaction(transaction);
        }

        public void NavigateToPage(Page content)
        {
            mainWindow.MainFrame.Content = content;
        }

        public async void OpenPopup2(Page content)
        {
            mainWindow.Popup2.Content = content;
            await WindowFade(mainWindow.Popup2, true);
        }

        public async void OpenPopup1(Page content)
        {
            mainWindow.Popup1.Content = content;
            await WindowFade(mainWindow.Popup1, true);
        }

        public async void ClosePopup2()
        {
            await WindowFade(mainWindow.Popup2, false);
            mainWindow.Popup2.Content = null;
        }
        public async void ClosePopups()
        {
            await WindowFade(mainWindow.Popup1, false);
            mainWindow.Popup1.Content = null;
            await WindowFade(mainWindow.Popup2, false);
            mainWindow.Popup1.Content = null;
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

            frame.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);

            await tcs.Task;

            if (!isOpening)
            {
                frame.Visibility = Visibility.Collapsed;
            }
        }
    }
}