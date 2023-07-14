using BNZApp.Pages;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

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

        private void OpenEditTransactionWindow(object sender, Transaction transaction1, Transaction transaction2)
        {

            Popup2.Content = null;
            EditTransactionWindow editTransactionWindow = new EditTransactionWindow(transaction1, transaction2);
            Popup1.Content = editTransactionWindow;

            editTransactionWindow.GoBack += BackToHomepage;
            editTransactionWindow.OpenAddReimbursementWindow += OpenAddReimbursementWindow;
            editTransactionWindow.OpenRemoveReimbursementWindow += OpenRemoveReimbursementWindow;
            editTransactionWindow.ReturnTransaction += ReturnTransaction;
        }

        private void ReturnTransaction(object sender, Transaction transaction)
        {
            homepage.ReturnTransaction(transaction);
        }
        private void OpenListWindow(object sender, List<ListItem> list, ListType type)
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            ListWindow listWindow = new ListWindow(list, type);
            Popup1.Content = listWindow;

            listWindow.GoBack += BackToHomepage;
        }

        private void OpenAddReimbursementWindow(object sender, Transaction transaction1, Transaction transaction2)
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

            addReimbursementWindow.GoBack += OpenEditTransactionWindow;
            addReimbursementWindow.GoBackHome += BackToHomepage;
        }

        private void OpenRemoveReimbursementWindow(object sender, Transaction transaction)
        {
            if (transaction is null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            RemoveReimbursementWindow removeReimbursementWindow = new RemoveReimbursementWindow(transaction);
            Popup2.Content = removeReimbursementWindow;

            removeReimbursementWindow.GoBack += OpenEditTransactionWindow;
            removeReimbursementWindow.GoBackHome += BackToHomepage;
        }

        private void OpenReimbursementListWindow(object sender, RoutedEventArgs e)
        {
            ReimbursementListWindow reimbursementListWindow = new ReimbursementListWindow();
            Popup1.Content = reimbursementListWindow;

            reimbursementListWindow.GoBack += BackToHomepage;
        }

        private void OpenUploadFile(object sender, RoutedEventArgs e)
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

        private void BackToHomepage(object sender, bool reloadPage)
        {
            if (reloadPage)
            {
                homepage.LoadPage();
            }
            Popup2.Content = null;
            Popup1.Content = null;
        }

        private void ClearData(object sender, RoutedEventArgs e)
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
            homepage.UploadFile += OpenUploadFile;
            homepage.ReimbursementListWindow += OpenReimbursementListWindow;
            homepage.ClearData += ClearData;
            homepage.OpenEditTransactionWindow += OpenEditTransactionWindow;
        }
    }
}
