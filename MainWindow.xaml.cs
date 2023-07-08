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

            homepage = new Homepage();
            MainFrame.Content = homepage;
            homepage.OpenViewListWindow += OpenViewListWindow;

        }

        private void OpenViewListWindow(object sender, List<string> list, TransItemType type)
        {
            ViewListWindow ViewListWindow = new ViewListWindow(list, type);
            PopUpWindow.Content = ViewListWindow;

            ViewListWindow.GoBack += BackToHomepage;
        }

        private void BackToHomepage(object sender, RoutedEventArgs e)
        {
            homepage.LoadPage();
            PopUpWindow.Content = null;
        }
    }
}
