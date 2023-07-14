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
    public partial class AddReimbursementWindow : Page
    {
        public event EventHandler<bool> GoBackHome;
        public event Action<object, Transaction, Transaction> GoBack;
        private List<Transaction> transactions;
        private Transaction transaction1;
        private Transaction transaction2;
        private List<Reimbursement> reimbursements = FileManagement.ReadReimbursements();
        public AddReimbursementWindow(Transaction transaction1, Transaction transaction2)
        {
            InitializeComponent();

            if (transaction1 is null)
            {
                throw new ArgumentNullException(nameof(transaction1), "First item cannot be null.");
            }

            if (transaction2 is null)
            {
                throw new ArgumentNullException(nameof(transaction2), "Second item cannot be null.");
            }

            this.transaction1 = transaction1;
            this.transaction2 = transaction2;

            transactions = new List<Transaction> { transaction1, transaction2 };
            ComparisonGrid.ItemsSource = transactions;
        }

        private void ConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            reimbursements.Add(new Reimbursement(transaction1, transaction2));
            FileManagement.WriteReimbursements(reimbursements);
            GoBackHome?.Invoke(sender, true);
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            GoBack?.Invoke(sender, transaction1, transaction2);
        }
    }
}
