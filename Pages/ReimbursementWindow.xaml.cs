using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for ReimbursementWindow.xaml
    /// </summary>
    public partial class ReimbursementWindow : Page
    {
        public event EventHandler<bool> GoBack;
        private bool isAdding;
        private List<Transaction> items;
        private Transaction firstItem;
        private Transaction secondItem;
        private Reimbursement reimbursement;
        public ReimbursementWindow(Transaction firstItem, Transaction secondItem)
        {
            InitializeComponent();

            if (firstItem is null || secondItem is null)
            {
                throw new ArgumentNullException(nameof(firstItem), nameof(secondItem));
            }

            this.firstItem = firstItem;
            this.secondItem = secondItem;

            isAdding = true;

            items = new List<Transaction> { firstItem, secondItem };
            Title.Text = "Confirm Reimbursement?";
            ComparisonGrid.ItemsSource = items;
        }
        public ReimbursementWindow(Transaction item)
        {
            InitializeComponent();

            if (item is null)
            {
                throw new NullReferenceException();
            }

            List<Reimbursement> reimbursements = FileManagement.ReadReimbursements();
            reimbursement = FindReimbursement(reimbursements, item);

            if (reimbursement is null)
            {
                throw new NullReferenceException();
            }

            isAdding = false;

            items = new List<Transaction> { reimbursement.transaction1, reimbursement.transaction2 };
            Title.Text = "Remove Reimbursement?";
            ComparisonGrid.ItemsSource = items;
        }
        private Reimbursement FindReimbursement(List<Reimbursement> reimbursements, Transaction item)
        {
            return reimbursements.FirstOrDefault(r => r.transaction1.id == item.id || r.transaction2.id == item.id);
        }

        private void ConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            if (isAdding)
            {
                Reimbursement reimbursement = new Reimbursement(firstItem, secondItem);
                FileManagement.WriteNewReimbursement(reimbursement);
            }
            else
            {
                List<Reimbursement> reimbursements = FileManagement.ReadReimbursements();
                Reimbursement reimbursement = FindReimbursement(reimbursements, firstItem);
                if (reimbursement != null)
                {
                    reimbursements.Remove(reimbursement);
                    FileManagement.WriteReimbursements(reimbursements);
                }
            }

            GoBack?.Invoke(sender, true);
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            GoBack?.Invoke(sender, false);
        }
    }
}
