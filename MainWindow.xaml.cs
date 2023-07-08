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
    public partial class MainWindow : Window
    {
        private Homepage homepage;
        public MainWindow()
        {
            InitializeComponent();

            homepage = new Homepage();
            MainFrame.Content = homepage;
            homepage.OpenSpendingPopup += OpenSpendingWindow;
            homepage.OpenIncomePopup += OpenIncomeWindow;
            homepage.OpenExpensesPopup += OpenExpensesWindow;
        }

        private void OpenSpendingWindow(object sender, List<string> listOfSpending)
        {
            SpendingWindow spendingWindow = new SpendingWindow(listOfSpending);
            PopUpWindow.Content = spendingWindow;

            spendingWindow.GoBack += BackToHomepage;
        }

        private void OpenIncomeWindow(object sender, List<string> listOfSpending)
        {

        }
        private void OpenExpensesWindow(object sender, List<string> listOfSpending)
        {

        }

        private void BackToHomepage(object sender, RoutedEventArgs e)
        {
            homepage.LoadPage();
            PopUpWindow.Content = null;
        }
    }
}
