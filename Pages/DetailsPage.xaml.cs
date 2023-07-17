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
            List<ListItem> income = listItems.Where(item => item.ListType == ListType.Income).ToList();
            List<ListItem> spending = listItems.Where(item => item.ListType == ListType.Spending).ToList();
            List<ListItem> expenses = listItems.Where(item => item.ListType == ListType.Expenses).ToList();

            List<DetailsItem> incomeItems = GetSource(income);
            List<DetailsItem> spendingItems = GetSource(spending);
            List<DetailsItem> expensesItems = GetSource(expenses);


            IncomeItemsGrid.ItemsSource = null;
            SpendingItemsGrid.ItemsSource = null;
            ExpensesItemsGrid.ItemsSource = null;
            IncomeItemsGrid.ItemsSource = incomeItems;
            TotalDecrease.Text = spendingItems.Sum(item => item.Amount).ToString("C");
            SpendingItemsGrid.ItemsSource = spendingItems;
            ExpensesItemsGrid.ItemsSource = expensesItems;
            Tax.Text = (-Homepage.TaxTotal).ToString("C");
        }

        private List<DetailsItem> GetSource(List<ListItem> listItems)
        {
            List<DetailsItem> detailsItems = new List<DetailsItem>();

            foreach (ListItem item in listItems)
            {
                IEnumerable<Transaction> matchingTransactions = FilterTransactions(item);
                decimal sum = CalculateSum(matchingTransactions);
                detailsItems.Add(new DetailsItem(item.Name, sum));
            }

            return detailsItems;
        }

        private IEnumerable<Transaction> FilterTransactions(ListItem item)
        {
            switch (item.Category)
            {
                case "payee":
                    return transactions.Where(transaction => transaction.Payee.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                case "particulars":
                    return transactions.Where(transaction => transaction.Particulars.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                case "code":
                    return transactions.Where(transaction => transaction.Code.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                case "reference":
                    return transactions.Where(transaction => transaction.Reference.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                default:
                    return Enumerable.Empty<Transaction>();
            }
        }

        private decimal CalculateSum(IEnumerable<Transaction> transactions)
        {
            decimal sum = transactions.Sum(transaction => transaction.Amount);
            decimal reimbursementSum = reimbursements.Sum(reimbursement => transactions.Sum(transaction => reimbursement.ExcludeFromTotal(transaction)));

            return sum - reimbursementSum;
        }

    }
}
