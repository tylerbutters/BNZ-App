using System;
using System.Collections.Generic;
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
        public event EventHandler<RoutedEventArgs> GoBack;
        private List<Transaction> items;
        private Transaction firstItem;
        private Transaction secondItem;
        public ReimbursementWindow(Transaction firstItem, Transaction secondItem)
        {
            InitializeComponent();

            if( firstItem is null || secondItem is null ) 
            {
                throw new NullReferenceException();
            }

            this.firstItem = firstItem;
            this.secondItem = secondItem;

            items = new List<Transaction>
            {
                firstItem,
                secondItem
            };

            ComparisonGrid.ItemsSource = items;
        }

        private void ConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            Reimbursement newReimbursement = new Reimbursement(firstItem, secondItem);
            FileManagement.WriteNewReimbursement(newReimbursement);
            GoBack?.Invoke(sender, e);
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            GoBack?.Invoke(sender, e);
        }
    }
}
