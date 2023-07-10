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
    /// Interaction logic for ViewReimbursementsWindow.xaml
    /// </summary>
    public partial class ViewReimbursementsWindow : Page
    {
        private bool isDeleteButtonClicked;
        private List<Reimbursement> reimbursements;
        public event EventHandler<bool> GoBack;

        public ViewReimbursementsWindow()
        {
            InitializeComponent();

            reimbursements = FileManagement.ReadReimbursements();

            if (reimbursements.Count is 0)
            {
                Console.Write("list of spending is empty");
            }

            ListGrid.ItemsSource = reimbursements;
        }

        private void DoneButtonClick(object sender, RoutedEventArgs e)
        {
            if (isDeleteButtonClicked)
            {
                FileManagement.WriteReimbursements(reimbursements);
            }

            GoBack?.Invoke(sender, isDeleteButtonClicked);
        }
        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            Reimbursement selectedReimbursement = (sender as Button)?.DataContext as Reimbursement;
            if (selectedReimbursement is null)
            {
                throw new NullReferenceException(nameof(selectedReimbursement));
            }
                reimbursements.Remove(selectedReimbursement);
                ListGrid.ItemsSource = null;
                ListGrid.ItemsSource = reimbursements;
                isDeleteButtonClicked = true;
        }
    }
}
