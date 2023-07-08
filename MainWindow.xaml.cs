using System.Collections.Generic;
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

            FileManagement.WriteTransactions(FileManagement.ReadTransactions());

            homepage = new Homepage();
            MainFrame.Content = homepage;
            homepage.OpenViewListWindow += OpenViewListWindow;
            homepage.OpenReimbursementWindow += OpenReimbursementWindow;

        }

        private void OpenViewListWindow(object sender, List<string> list, TransItemType type)
        {
            ViewListWindow ViewListWindow = new ViewListWindow(list, type);
            PopUpWindow.Content = ViewListWindow;

            ViewListWindow.GoBack += BackToHomepage;
        }

        private void OpenReimbursementWindow(object sender, Transaction firstItem, Transaction secondItem)
        {
            ReimbursementWindow ReimbursementWindow = new ReimbursementWindow(firstItem, secondItem);
            PopUpWindow.Content = ReimbursementWindow;

            ReimbursementWindow.GoBack += BackToHomepage;
        }

        private void BackToHomepage(object sender, RoutedEventArgs e)
        {
            homepage.LoadPage();
            PopUpWindow.Content = null;
        }
    }
}
