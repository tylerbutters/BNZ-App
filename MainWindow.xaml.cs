using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

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
            ViewListWindow viewListWindow = new ViewListWindow(list, type);
            PopUpWindow.Content = viewListWindow;

            viewListWindow.GoBack += BackToHomepage;
        }

        private void OpenReimbursementWindow_Add(object sender, Transaction firstItem, Transaction secondItem)
        {
            ReimbursementWindow reimbursementWindow = new ReimbursementWindow(firstItem, secondItem);
            PopUpWindow.Content = reimbursementWindow;

            reimbursementWindow.GoBack += BackToHomepage;
        }

        private void OpenReimbursementWindow_Remove(object sender, Transaction item)
        {
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

            bool? result = openFileDialog.ShowDialog();

            if (result is true) //if user selected a file
            {
                string selectedFilePath = openFileDialog.FileName;
                File.Copy(selectedFilePath, FileManagement.TransactionsFile, true);
                List<Transaction> newTransactions = FileManagement.ReadNewTransactions();
                if(newTransactions is null)
                {
                    return;
                }

                
                
                FileManagement.WriteTransactions(newTransactions);

                BackToHomepage(sender, true);
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
            homepage.OpenViewListWindow += OpenViewListWindow;
            homepage.OpenReimbursementWindow_Add += OpenReimbursementWindow_Add;
            homepage.OpenReimbursementWindow_Remove += OpenReimbursementWindow_Remove;
            homepage.UploadFile += OpenUploadFile;
            homepage.ViewReimbursementsWindow += OpenViewReimbursementsWindow;
            homepage.ClearData += ClearData;
        }
    }
}
