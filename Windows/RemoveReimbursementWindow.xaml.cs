﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for RemoveReimbursementWindow.xaml
    /// </summary>
    public partial class RemoveReimbursementWindow : Page
    {
        public event Action GoBack;
        public event Action<bool> GoBackHome;
        private Reimbursement reimbursement;
        private List<Reimbursement> reimbursements = FileManagement.ReadReimbursements();

        public RemoveReimbursementWindow(Transaction transaction)
        {
            InitializeComponent();

            if (transaction is null)
            {
                throw new ArgumentNullException(nameof(transaction), "Item cannot be null.");
            }

            reimbursement = FindReimbursement(reimbursements, transaction);

            if (reimbursement is null)
            {
                throw new NullReferenceException("Reimbursement not found for the given item.");
            }

            ComparisonGrid.ItemsSource = new List<Transaction> { reimbursement.Transaction1, reimbursement.Transaction2 };
        }

        private void BackgroundClick(object sender, MouseButtonEventArgs e)
        {
            CancelButtonClick(sender, e);
        }

        private Reimbursement FindReimbursement(List<Reimbursement> reimbursements, Transaction transaction)
        {
            if (transaction is null)
            {
                throw new ArgumentNullException("Item is null", nameof(transaction));
            }
            return reimbursements.FirstOrDefault(reimbursement => transaction.Equals(reimbursement.Transaction1) || transaction.Equals(reimbursement.Transaction2));
        }

        private void ConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            if (reimbursement != null)
            {
                reimbursements.Remove(reimbursement);
                FileManagement.WriteReimbursements(reimbursements);
            }
            GoBackHome?.Invoke(true);
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            GoBack?.Invoke();
        }
    }
}
