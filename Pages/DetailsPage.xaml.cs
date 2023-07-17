using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for DetailsPage.xaml
    /// </summary>
    public partial class DetailsPage : Page
    {
        private List<Transaction> transactions;
        private List<Reimbursement> reimbursements = FileManagement.ReadReimbursements();
        public DetailsPage()
        {
            InitializeComponent();
        }

        public void UpdateDetails(List<Transaction> transactions, List<ListItem> listItems)
        {
            this.transactions = transactions;
            List<DetailsItem> incomeItems = GetSource(transactions, listItems, ListType.Income);
            List<DetailsItem> spendingItems = GetSource(transactions, listItems, ListType.Spending);
            List<DetailsItem> expensesItems = GetSource(transactions, listItems, ListType.Expenses);

            IncomeItemsGrid.ItemsSource = incomeItems;
            TotalDecrease.Text = spendingItems.Sum(item => item.Amount).ToString("C");
            SpendingItemsGrid.ItemsSource = spendingItems;
            ExpensesItemsGrid.ItemsSource = expensesItems;
            Tax.Text = (-Homepage.TaxTotal).ToString("C");
        }

        private List<DetailsItem> GetSource(List<Transaction> transactions, List<ListItem> listItems, ListType itemType)
        {
            List<DetailsItem> detailsItems = listItems
                .Where(item => item.ListType == itemType)
                .Select(item => new DetailsItem(item.Name, CalculateSum(transactions, item)))
                .ToList();

            return detailsItems;
        }

        private decimal CalculateSum(List<Transaction> transactions, ListItem item)
        {
            List<Transaction> matchingTransactions = GetMatchingTransactions(transactions, item);
            decimal sum = matchingTransactions.Sum(transaction => transaction.Amount);
            decimal excludedAmount = ExcludeReimbursements(matchingTransactions);
            decimal total = sum - excludedAmount;

            return total;
        }

        private decimal ExcludeReimbursements(List<Transaction> transactions)
        {
            decimal excludedAmount = 0;
            foreach (Reimbursement reimbursement in reimbursements)
            {
                foreach (Transaction transaction in transactions)
                {
                    excludedAmount -= reimbursement.ExcludeAmount(transaction);
                }
            }

            return excludedAmount;
        }

        private List<Transaction> GetMatchingTransactions(List<Transaction> transactions, ListItem item)
        {
            switch (item.Category)
            {
                case "payee":
                    return transactions.Where(transaction => transaction.Payee.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                case "particulars":
                    return transactions.Where(transaction => transaction.Particulars.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                case "code":
                    return transactions.Where(transaction => transaction.Code.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                case "reference":
                    return transactions.Where(transaction => transaction.Reference.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                default:
                    return new List<Transaction>();
            }
        }
    }
}
