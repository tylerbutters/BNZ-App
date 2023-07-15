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
            List<ListItem> income = listItems.Where(item => item.listType == ListType.Income).ToList();
            List<ListItem> spending = listItems.Where(item => item.listType == ListType.Spending).ToList();
            List<ListItem> expenses = listItems.Where(item => item.listType == ListType.Expenses).ToList();

            List<DetailsItem> expensesItems = new List<DetailsItem> { new DetailsItem("tax", -Homepage.taxTotal) };
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
                switch (item.category)
                {
                    case "payee":
                        float sum = transactions.Where(transaction => transaction.payee.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0).Sum(transaction => transaction.amount);
                        detailsItems.Add(new DetailsItem(item.name, sum));
                        break;
                    case "particulars":
                        sum = transactions.Where(transaction => transaction.particulars.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0).Sum(transaction => transaction.amount);
                        detailsItems.Add(new DetailsItem(item.name, sum));
                        break;
                    case "code":
                        sum = transactions.Where(transaction => transaction.code.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0).Sum(transaction => transaction.amount);
                        detailsItems.Add(new DetailsItem(item.name, sum));
                        break;
                    case "reference":
                        sum = transactions.Where(transaction => transaction.reference.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0).Sum(transaction => transaction.amount);
                        detailsItems.Add(new DetailsItem(item.name, sum));
                        break;
                }
            }

            return detailsItems;
        }
    }
}
