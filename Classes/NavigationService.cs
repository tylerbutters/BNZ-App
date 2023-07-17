using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace BNZApp
{
    public class NavigationService
    {
        private MainWindow mainWindow;
        private Homepage homepage;
        private LoginPage loginPage = new LoginPage();

        public NavigationService(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            mainWindow.SideNav.Visibility = Visibility.Collapsed;
            mainWindow.OpenWelcomePage += OpenWelcomePage;
            mainWindow.OpenReimbursementListWindow += OpenReimbursementListWindow;
            mainWindow.UploadFile += UploadFile;

            if (FileManagement.ReadTransactions() is null)
            {
                OpenWelcomePage();
            }
            else
            {
                mainWindow.NavigateToPage(loginPage);
                loginPage.OpenHomepage += CreateHomepage;
            }
        }

        private void CreateHomepage()
        {
            homepage = new Homepage();
            homepage.OpenListWindow += OpenListWindow;
            homepage.TransactionGridPage.OpenEditTransactionWindow += OpenEditTransactionWindow;
            mainWindow.SideNav.Visibility = Visibility.Visible;
            mainWindow.NavigateToPage(homepage);
        }

        public void OpenWelcomePage()
        {
            WelcomePage welcomePage = new WelcomePage();
            welcomePage.UploadFile += UploadFile;
            mainWindow.NavigateToPage(welcomePage);
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

        private void OpenEditTransactionWindow(Transaction transaction1, Transaction transaction2)
        {
            EditTransactionWindow editTransactionWindow = new EditTransactionWindow(transaction1, transaction2);
            editTransactionWindow.GoBack += BackToHomepage;
            editTransactionWindow.OpenAddReimbursementWindow += OpenAddReimbursementWindow;
            editTransactionWindow.OpenRemoveReimbursementWindow += OpenRemoveReimbursementWindow;
            editTransactionWindow.ReturnTransaction += ReturnTransaction;
            mainWindow.OpenPopup1(editTransactionWindow);
        }

        private EditItemWindow editItemWindow;
        private ListWindow listWindow;

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
                mainWindow.ClosePopup2();
            };
            mainWindow.OpenPopup2(editItemWindow);
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
            mainWindow.OpenPopup1(listWindow);
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
            addReimbursementWindow.GoBack += () => mainWindow.ClosePopup2();
            addReimbursementWindow.GoBackHome += BackToHomepage;
            mainWindow.OpenPopup2(addReimbursementWindow);
        }

        private void OpenRemoveReimbursementWindow(Transaction transaction)
        {
            if (transaction is null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            RemoveReimbursementWindow removeReimbursementWindow = new RemoveReimbursementWindow(transaction);
            removeReimbursementWindow.GoBack += () => mainWindow.ClosePopup2();
            removeReimbursementWindow.GoBackHome += BackToHomepage;
            mainWindow.OpenPopup2(removeReimbursementWindow);
        }

        private void OpenReimbursementListWindow()
        {
            ReimbursementListWindow reimbursementListWindow = new ReimbursementListWindow();
            reimbursementListWindow.GoBack += BackToHomepage;
            mainWindow.OpenPopup1(reimbursementListWindow);
        }

        private void BackToHomepage(bool reloadPage)
        {
            if (reloadPage)
            {
                homepage.LoadPage();
            }
            mainWindow.ClosePopups();
        }

        private void ReturnTransaction(Transaction transaction)
        {
            homepage.TransactionGridPage.ReturnTransaction(transaction);
        }
    }
}