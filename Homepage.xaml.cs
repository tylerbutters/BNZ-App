using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for Homepage.xaml
    /// </summary>
    public partial class Homepage : Page, INotifyPropertyChanged
    {
        private DateTime currentDate;
        private List<string> listOfIncome;
        private List<string> listOfSpending;
        private List<string> listOfExpenses;
        private List<Reimbursement> reimbursements;
        private List<Transaction> transactions;
        private List<Transaction> currentWeekTransactions;
        private IEnumerable<IGrouping<int, Transaction>> groupedTransactions;
        private string formattedTotalIncome;
        private string formattedTotalSpending;
        private string formattedTotalExpenses;
        private string formattedTotalIncrease;
        private string formattedTotalDecrease;
        private float totalIncome;
        private float totalSpending;
        private float totalExpenses;
        private float totalIncrease;
        private float totalDecrease;
        public string FormattedTotalIncome { get => formattedTotalIncome; set { formattedTotalIncome = value; OnPropertyChanged(nameof(FormattedTotalIncome)); } }
        public string FormattedTotalSpending { get => formattedTotalSpending; set { formattedTotalSpending = value; OnPropertyChanged(nameof(FormattedTotalSpending)); } }
        public string FormattedTotalExpenses { get => formattedTotalExpenses; set { formattedTotalExpenses = value; OnPropertyChanged(nameof(FormattedTotalExpenses)); } }
        public string FormattedTotalIncrease { get => formattedTotalIncrease; set { formattedTotalIncrease = value; OnPropertyChanged(nameof(FormattedTotalIncrease)); } }
        public string FormattedTotalDecrease { get => formattedTotalDecrease; set { formattedTotalDecrease = value; OnPropertyChanged(nameof(FormattedTotalDecrease)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<object, List<string>, TransItemType> OpenViewListWindow;
        public event Action<object, Transaction, Transaction> OpenReimbursementWindow;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Homepage()
        {
            InitializeComponent();

            DataContext = this;
            transactions = FileManagement.ReadTransactions();
            currentDate = transactions.Max(transaction => transaction.date);
            LoadPage();
        }
        public void LoadPage()
        {
            GetData();
            UpdateUI();
        }
        private void GetData()
        {
            reimbursements = FileManagement.ReadReimbursements();
            listOfIncome = FileManagement.ReadList(TransItemType.Income);
            listOfSpending = FileManagement.ReadList(TransItemType.Spending);
            listOfExpenses = FileManagement.ReadList(TransItemType.Expenses);

            if (transactions != null)
            {
                groupedTransactions = transactions.GroupBy(transaction => GetWeekNumber(transaction.date));
            }
        }

        private int GetWeekNumber(DateTime date)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }

        private float GetTotal(List<Transaction> transactions, List<string> items)
        {
            if (items is null)
            {
                return 0;
            }

            List<Transaction> matchingTransactions = transactions.Where(transaction => items.Any(item => transaction.payee.Contains(item))).ToList();
            float sum = matchingTransactions.Sum(transaction => transaction.amount);

            // Subtract the amounts of negative reimbursement transactions
            foreach (Reimbursement reimbursement in reimbursements)
            {
                if (matchingTransactions.Any(transaction => transaction.id == reimbursement.transaction1.id && transaction.amount < 0))
                {
                    sum -= reimbursement.transaction1.amount;
                }
                else if (matchingTransactions.Any(transaction => transaction.id == reimbursement.transaction2.id && transaction.amount < 0))
                {
                    sum -= reimbursement.transaction2.amount;
                }
            }

            return sum;
        }

        private void ForwardButtonClick(object sender, RoutedEventArgs e)
        {
            DateTime newDate = currentDate.AddDays(7);

            bool hasTransactions = transactions.Any(transaction => transaction.date.Date == newDate.Date);

            if (!hasTransactions)
            {
                MessageBox.Show("No transactions found for the selected week.", "No Transactions");
                return;
            }

            currentDate = newDate;
            UpdateUI();
        }

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            DateTime newDate = currentDate.AddDays(-7);

            bool hasTransactions = transactions.Any(transaction => transaction.date.Date == newDate.Date);

            if (!hasTransactions)
            {
                MessageBox.Show("No transactions found for the selected week.", "No Transactions");
                return;
            }

            currentDate = newDate;
            UpdateUI();
        }

        private void UpdateUI()
        {
            UpdateWeekNumberDisplay();
            UpdateCurrentWeekGroup();
            UpdateTransactionGrid();
            UpdateSummary();
        }

        private void UpdateWeekNumberDisplay()
        {
            WeekNumber.Text = GetWeekNumber(currentDate).ToString();
            Month.Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(currentDate.Month);
        }

        private void UpdateCurrentWeekGroup()
        {
            if (groupedTransactions != null)
            {
                IGrouping<int, Transaction> currentWeekGroup = groupedTransactions.FirstOrDefault(group => group.Key == GetWeekNumber(currentDate));
                currentWeekTransactions = currentWeekGroup?.ToList() ?? new List<Transaction>();
            }
        }

        private void UpdateTransactionGrid()
        {
            if (currentWeekTransactions != null)
            {
                TransactionGrid.ItemsSource = currentWeekTransactions;
            }
        }
        private void UpdateSummary()
        {
            if (currentWeekTransactions != null)
            {
                totalIncome = GetTotal(currentWeekTransactions, listOfIncome);
                totalSpending = GetTotal(currentWeekTransactions, listOfSpending);
                totalExpenses = GetTotal(currentWeekTransactions, listOfExpenses) + (totalIncome / -10);
                totalIncrease = totalIncome + totalSpending + totalExpenses;
                totalDecrease = currentWeekTransactions.Where(transaction => transaction.amount < 0).Sum(transaction => transaction.amount);

                FormattedTotalIncome = totalIncome.ToString("C");
                FormattedTotalSpending = totalSpending.ToString("C");
                FormattedTotalExpenses = totalExpenses.ToString("C");
                FormattedTotalDecrease = totalDecrease.ToString("C");
                FormattedTotalIncrease = totalIncrease.ToString("C");
                Tithing.Text = (totalIncome / -10).ToString("C");
            }
        }

        private void ViewIncomeClick(object sender, RoutedEventArgs e)
        {
            OpenViewListWindow?.Invoke(sender, listOfIncome, TransItemType.Income);
        }

        private void ViewSpendingClick(object sender, RoutedEventArgs e)
        {
            OpenViewListWindow?.Invoke(sender, listOfSpending, TransItemType.Spending);
        }

        private void ViewExpensesClick(object sender, RoutedEventArgs e)
        {
            OpenViewListWindow?.Invoke(sender, listOfExpenses, TransItemType.Expenses);
        }

        private Transaction firstItemClicked;
        private Transaction secondItemClicked;

        private void TransactionGridItemClick(object sender, RoutedEventArgs e)
        {
            Transaction selectedItem = (e.OriginalSource as FrameworkElement)?.DataContext as Transaction;

            if (selectedItem is null)
            {
                throw new NullReferenceException();
            }

            if (firstItemClicked is null)
            {
                selectedItem.IsItemClicked = true;
                firstItemClicked = selectedItem;
            }
            else if (firstItemClicked == selectedItem)
            {
                selectedItem.IsItemClicked = false;
                firstItemClicked = null;
            }
            else if (secondItemClicked is null && firstItemClicked != null)
            {
                secondItemClicked = selectedItem;
                bool isValidReimbursement = (secondItemClicked.amount > 0 && firstItemClicked.amount < 0) || (secondItemClicked.amount < 0 && firstItemClicked.amount > 0);
                if (!isValidReimbursement)
                {
                    MessageBox.Show("One transaction needs to be positive, and the other needs to be negative.");
                    secondItemClicked = null;
                    return;
                }

                OpenReimbursementWindow?.Invoke(sender, firstItemClicked, secondItemClicked);
                firstItemClicked.IsItemClicked = false;
                firstItemClicked = null;
                secondItemClicked = null;
            }
        }

    }
}