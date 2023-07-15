using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for TransactionGridPage.xaml
    /// </summary>
    public partial class TransactionGridPage : Page
    {
        private List<Reimbursement> reimbursements = FileManagement.ReadReimbursements();
        private Transaction selectedTransaction;
        private Transaction stagedForReimbursement;
        public event Action<object, Transaction, Transaction> OpenEditTransactionWindow;
        public TransactionGridPage()
        {
            InitializeComponent();
        }
        public void UpdateTransactionGrid(List<Transaction> transactions)
        {
            if (transactions is null)
            {
                NoResultsText.Visibility = Visibility.Visible;
                return;
            }
            stagedForReimbursement = null;
            TransactionGrid.ItemsSource = null;
            TransactionGrid.ItemsSource = transactions;
        }

        public void ReturnTransaction(Transaction transaction)
        {
            if (stagedForReimbursement is null)
            {
                stagedForReimbursement = transaction;
            }
        }
        private void TransactionGridItemClick(object sender, RoutedEventArgs e)
        {
            selectedTransaction = (sender as FrameworkElement)?.DataContext as Transaction;

            if (selectedTransaction is null)
            {
                throw new NullReferenceException("Selected item is null.");
            }
            foreach (Reimbursement reimbursement in reimbursements)
            {
                if (selectedTransaction.Equals(reimbursement.Transaction1) || selectedTransaction.Equals(reimbursement.Transaction2))
                {
                    OpenEditTransactionWindow?.Invoke(sender, reimbursement.Transaction1, reimbursement.Transaction2);
                }
            }
            if (selectedTransaction.Equals(stagedForReimbursement))
            {
                OpenEditTransactionWindow?.Invoke(sender, stagedForReimbursement, null);
            }
            else if (stagedForReimbursement != null)
            {
                OpenEditTransactionWindow?.Invoke(sender, selectedTransaction, stagedForReimbursement);
            }
            else
            {
                OpenEditTransactionWindow?.Invoke(sender, selectedTransaction, null);
            }
        }
    }
}
