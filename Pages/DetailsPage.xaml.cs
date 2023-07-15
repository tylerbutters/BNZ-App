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

            List<DetailsItem> expensesItems = new List<DetailsItem> { new DetailsItem("tax", -Homepage.TaxTotal) };
            expensesItems.AddRange(GetSource(expenses));

            IncomeItemsGrid.ItemsSource = null;
            SpendingItemsGrid.ItemsSource = null;
            ExpensesItemsGrid.ItemsSource = null;
            IncomeItemsGrid.ItemsSource = GetSource(income);
            SpendingItemsGrid.ItemsSource = GetSource(spending);
            ExpensesItemsGrid.ItemsSource = expensesItems;
        }

        private List<DetailsItem> GetSource(List<ListItem> listItems)
        {
            List<DetailsItem> detailsItems = new List<DetailsItem>();
            foreach (ListItem item in listItems)
            {
                switch (item.Category)
                {
                    case "payee":
                        decimal sum = transactions.Where(transaction => transaction.Payee.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0).Sum(transaction => transaction.Amount);
                        detailsItems.Add(new DetailsItem(item.Name, sum));
                        break;
                    case "particulars":
                        sum = transactions.Where(transaction => transaction.Particulars.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0).Sum(transaction => transaction.Amount);
                        detailsItems.Add(new DetailsItem(item.Name, sum));
                        break;
                    case "code":
                        sum = transactions.Where(transaction => transaction.Code.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0).Sum(transaction => transaction.Amount);
                        detailsItems.Add(new DetailsItem(item.Name, sum));
                        break;
                    case "reference":
                        sum = transactions.Where(transaction => transaction.Reference.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0).Sum(transaction => transaction.Amount);
                        detailsItems.Add(new DetailsItem(item.Name, sum));
                        break;
                }
            }

            return detailsItems;
        }
    }
}
