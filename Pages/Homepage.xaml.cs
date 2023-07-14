using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for Homepage.xaml
    /// </summary>
    public partial class Homepage : Page
    {
        public static float taxPercentage = 0.105f;
        public string formattedTotalIncome { get => totalIncome.ToString("C"); set { totalIncome = float.Parse(value); } }
        public string formattedTotalSpending { get => totalSpending.ToString("C"); set { totalSpending = float.Parse(value); } }
        public string formattedTotalExpenses { get => totalExpenses.ToString("C"); set { totalExpenses = float.Parse(value); } }
        public string formattedTotalDecrease { get => totalDecrease.ToString("C"); set { totalDecrease = float.Parse(value); } }
        public string formattedTotal { get => total.ToString("C"); set { total = float.Parse(value); } }
        public bool totalIsNegative { get => total < 0; set { totalIsNegative = value; } }
        private float totalIncome;
        private float totalSpending;
        private float totalExpenses;
        private float totalDecrease;
        private float total;

        private DateTime currentDate;
        private DateTime latestDate;
        private List<ListItem> listOfItems;
        private List<Reimbursement> reimbursements;
        private List<Transaction> transactions = FileManagement.ReadTransactions();
        private List<Transaction> currentWeekTransactions;
        private Transaction stagedForReimbursement;
        private IEnumerable<IGrouping<int, Transaction>> groupedTransactions;

        public event Action<object, List<ListItem>, ListType> OpenListWindow;
        public event Action<object, Transaction, Transaction> OpenEditTransactionWindow;
        public event EventHandler<RoutedEventArgs> ReimbursementListWindow;
        public event EventHandler<RoutedEventArgs> UploadFile;
        public event EventHandler<RoutedEventArgs> ClearData;
        public Homepage()
        {
            InitializeComponent();

            DataContext = this;

            if (transactions is null)
            {
                NoResultsText.Visibility = Visibility.Visible;
                return;
            }

            latestDate = transactions.Max(transaction => transaction.date);
            currentDate = latestDate;
            LoadPage();
        }

        public void LoadPage()
        {
            stagedForReimbursement = null;
            GetData();
            UpdateUI();
        }

        public void GetData()
        {
            transactions = FileManagement.ReadTransactions();
            reimbursements = FileManagement.ReadReimbursements();
            listOfItems = FileManagement.ReadList();

            groupedTransactions = transactions.GroupBy(transaction => GetWeekNumber(transaction.date));
        }

        public void UpdateUI()
        {
            if (currentDate == latestDate)
            {
                LatestButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                LatestButton.Visibility = Visibility.Visible;
            }
            GetNewWeekTransactions();
            UpdateWeekNumberDisplay();
            UpdateTransactionGrid();
            UpdateSummary();
        }

        private void UpdateWeekNumberDisplay()
        {
            string startDate = GetStartDateOfWeek(currentDate).ToString("dd/MM/yy");
            string endDate = DateTime.Parse(startDate).AddDays(7).ToString("dd/MM/yy");
            string month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(currentDate.Month);
            string weekNumber = GetWeekNumber(currentDate).ToString();
            WeekAndMonth.Text = month + ", Week " + weekNumber;
            Dates.Text = startDate + " - " + endDate;
        }

        private void UpdateTransactionGrid()
        {
            TransactionGrid.ItemsSource = null;
            TransactionGrid.ItemsSource = currentWeekTransactions;
        }

        private void UpdateSummary()
        {
            if (currentWeekTransactions is null)
            {
                throw new NullReferenceException("Current week transactions is null.");
            }

            totalIncome = GetTotal(currentWeekTransactions, listOfItems, ListType.Income);
            float tax = totalIncome * taxPercentage;
            totalSpending = GetTotal(currentWeekTransactions, listOfItems, ListType.Spending);
            totalExpenses = GetTotal(currentWeekTransactions, listOfItems, ListType.Expenses) - tax;
            total = totalIncome + totalSpending + totalExpenses;
            totalDecrease = currentWeekTransactions.Where(transaction => transaction.amount < 0).Sum(transaction => transaction.amount);
            Tax.Text = (totalIncome * taxPercentage).ToString("C");

            DataContext = null;
            DataContext = this;
        }

        private int GetWeekNumber(DateTime date)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }

        private DateTime GetStartDateOfWeek(DateTime currentDate)
        {
            DayOfWeek firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            int diff = (currentDate.DayOfWeek - firstDayOfWeek + 7) % 7;
            return currentDate.AddDays(-1 * diff).Date;
        }

        private float GetTotal(List<Transaction> transactions, List<ListItem> items, ListType itemType)
        {
            if (items is null)
            {
                return 0;
            }

            List<Transaction> matchingTransactions = GetMatchingTransactions(transactions, items, itemType);

            if (matchingTransactions.Count is 0)
            {
                return 0;
            }

            if (itemType == ListType.Income)
            {
                return matchingTransactions.Sum(transaction => transaction.amount);
            }
            else
            {
                return CalculateSum(matchingTransactions);
            }
        }

        private List<Transaction> GetMatchingTransactions(List<Transaction> transactions, List<ListItem> items, ListType itemType)
        {
            List<Transaction> matchingTransactions = new List<Transaction>();

            foreach (ListItem item in items)
            {
                if (item.listType == itemType)
                {
                    IEnumerable<Transaction> filteredTransactions;

                    switch (item.category)
                    {
                        case "payee":
                            filteredTransactions = transactions.Where(transaction => transaction.payee.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0);
                            break;
                        case "particulars":
                            filteredTransactions = transactions.Where(transaction => transaction.particulars.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0);
                            break;
                        case "code":
                            filteredTransactions = transactions.Where(transaction => transaction.code.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0);
                            break;
                        case "reference":
                            filteredTransactions = transactions.Where(transaction => transaction.reference.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0);
                            break;
                        default:
                            throw new ArgumentException("Item type is not valid", nameof(item.category));
                    }

                    if (itemType != ListType.Income)
                    {
                        filteredTransactions = filteredTransactions.Where(transaction => transaction.amount < 0);
                    }

                    matchingTransactions.AddRange(filteredTransactions);
                }
            }

            return matchingTransactions;
        }

        private float CalculateSum(List<Transaction> transactions)
        {
            float sum = transactions.Sum(transaction => transaction.amount);

            foreach (Reimbursement reimbursement in reimbursements)
            {
                foreach (Transaction transaction in transactions)
                {
                    sum -= reimbursement.ExcludeFromTotal(transaction);                  
                }
            }

            return sum;
        }

        private void GetNewWeekTransactions()
        {
            if (groupedTransactions is null)
            {
                throw new NullReferenceException("Grouped transactions is null.");
            }

            List<Transaction> newWeekTransactions = groupedTransactions.FirstOrDefault(group => group.Key == GetWeekNumber(currentDate)).ToList();

            if (newWeekTransactions is null)
            {
                throw new NullReferenceException("Current week group not found.");
            }

            if (newWeekTransactions.Count is 0)
            {
                MessageBox.Show("No transactions found for the selected week.", "No Transactions");
                return;
            }

            currentWeekTransactions = newWeekTransactions;
        }

        private void ForwardButtonClick(object sender, RoutedEventArgs e)
        {
            if (transactions is null)
            {
                MessageBox.Show("No Transactions in file");
                return;
            }
            DateTime newDate = currentDate.AddDays(7);
            stagedForReimbursement = null;
            bool hasTransactions = groupedTransactions.Any(group => group.Key == GetWeekNumber(newDate));

            if (!hasTransactions)
            {
                MessageBox.Show("No transactions found for the selected week.", "No Transactions");
                return;
            }
            currentDate = newDate;
            LoadPage();
        }
        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            if (transactions.Count is 0)
            {
                MessageBox.Show("No transactions found for the selected week.", "No Transactions");
                return;
            }
            DateTime newDate = currentDate.AddDays(-7);
            stagedForReimbursement = null;
            bool hasTransactions = groupedTransactions.Any(group => group.Key == GetWeekNumber(newDate));

            if (!hasTransactions)
            {
                MessageBox.Show("No transactions found for the selected week.", "No Transactions");
                return;
            }

            currentDate = newDate;
            LoadPage();
        }

        private void LatestButtonClick(object sender, RoutedEventArgs e)
        {
            currentDate = latestDate;
            UpdateUI();
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ClearDataButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to clear all your data?", "Confirm", MessageBoxButton.YesNo);

            if (result is MessageBoxResult.Yes)
            {
                ClearData?.Invoke(sender, e);
            }
        }

        private void ReimbursementsButtonClick(object sender, RoutedEventArgs e)
        {
            ReimbursementListWindow?.Invoke(sender, e);
        }

        private void UploadFileButtonClick(object sender, RoutedEventArgs e)
        {
            UploadFile?.Invoke(sender, e);
        }

        private void ViewListClick(object sender, RoutedEventArgs e)
        {
            if (transactions.Count is 0)
            {
                MessageBox.Show("Please upload transactions file first");
                return;
            }
            var border = (sender as Border);
            ListType type;
            switch (border.Tag)
            {
                case "Income":
                    type = ListType.Income;
                    break;
                case "Spending":
                    type = ListType.Spending;
                    break;
                case "Expenses":
                    type = ListType.Expenses;
                    break;
                default:
                    throw new ArgumentException("Invalid button tag.");
            }
            OpenListWindow?.Invoke(sender, listOfItems, type);
        }

        private Transaction selectedTransaction;

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
                if (selectedTransaction.Equals(reimbursement.transaction1) || selectedTransaction.Equals(reimbursement.transaction2))
                {
                    OpenEditTransactionWindow?.Invoke(sender, reimbursement.transaction1, reimbursement.transaction2);
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

            UpdateUI();
        }
    }
}