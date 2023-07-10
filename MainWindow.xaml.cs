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
    public enum TransItemType
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

        private void OpenViewListWindow(object sender, List<string> list, TransItemType type)
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            ViewListWindow viewListWindow = new ViewListWindow(list, type);
            PopUpWindow.Content = viewListWindow;

            viewListWindow.GoBack += BackToHomepage;
        }

        private void OpenReimbursementWindow_Add(object sender, Transaction firstItem, Transaction secondItem)
        {
            if (firstItem is null)
            {
                throw new ArgumentNullException(nameof(firstItem));
            }

            if (secondItem is null)
            {
                throw new ArgumentNullException(nameof(secondItem));
            }

            ReimbursementWindow reimbursementWindow = new ReimbursementWindow(firstItem, secondItem);
            PopUpWindow.Content = reimbursementWindow;

            reimbursementWindow.GoBack += BackToHomepage;
        }

        private void OpenReimbursementWindow_Remove(object sender, Transaction item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            ReimbursementWindow reimbursementWindow = new ReimbursementWindow(item);
            PopUpWindow.Content = reimbursementWindow;

            reimbursementWindow.GoBack += BackToHomepage;
        }

        private void OpenViewReimbursementsWindow(object sender, RoutedEventArgs e)
        {
            ViewReimbursementsWindow viewReimbursementsWindow = new ViewReimbursementsWindow();
            PopUpWindow.Content = viewReimbursementsWindow;

            viewReimbursementsWindow.GoBack += BackToHomepage;
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
                File.Copy(selectedFilePath, FileManagement.TransactionsFile, true);
                //List<Transaction> newTransactions = FileManagement.ReadNewFile(); //for release
                List<Transaction> newTransactions = FileManagement.ReadTransactions(); //for debugging
                if (newTransactions is null)
                {
                    throw new InvalidOperationException("Failed to read new transactions from the selected file.");
                }
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

            PopUpWindow.Content = null;
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
            PopUpWindow.Content = null;
            homepage.OpenViewListWindow += OpenViewListWindow;
            homepage.OpenReimbursementWindow_Add += OpenReimbursementWindow_Add;
            homepage.OpenReimbursementWindow_Remove += OpenReimbursementWindow_Remove;
            homepage.UploadFile += OpenUploadFile;
            homepage.ViewReimbursementsWindow += OpenViewReimbursementsWindow;
            homepage.ClearData += ClearData;
        }

    }
}
