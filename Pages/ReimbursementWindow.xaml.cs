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
        private List<Reimbursement> reimbursements = FileManagement.ReadReimbursements();
        public ReimbursementWindow(Transaction firstItem, Transaction secondItem)
        {
            InitializeComponent();

            if (firstItem is null)
            {
                throw new ArgumentNullException(nameof(firstItem), "First item cannot be null.");
            }

            if (secondItem is null)
            {
                throw new ArgumentNullException(nameof(secondItem), "Second item cannot be null.");
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
                throw new ArgumentNullException(nameof(item), "Item cannot be null.");
            }

            firstItem = item;

            reimbursement = FindReimbursement(reimbursements, firstItem);

            if (reimbursement is null)
            {
                throw new NullReferenceException("Reimbursement not found for the given item.");
            }

            isAdding = false;

            items = new List<Transaction> { reimbursement.transaction1, reimbursement.transaction2 };
            Title.Text = "Remove Reimbursement?";
            ComparisonGrid.ItemsSource = items;
        }

        private Reimbursement FindReimbursement(List<Reimbursement> reimbursements, Transaction item)
        {
            if(item is null)
            {
                throw new ArgumentNullException("Item is null", nameof(item));
            }
            return reimbursements.FirstOrDefault(reimbursement => item.Equals(reimbursement.transaction1) || item.Equals(reimbursement.transaction2));
        }

        private void ConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            if (isAdding)
            {
                reimbursements.Add(new Reimbursement(firstItem, secondItem));
                FileManagement.WriteReimbursements(reimbursements);
            }
            else
            {
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
