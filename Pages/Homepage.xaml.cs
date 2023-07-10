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
    public partial class Homepage : Page, INotifyPropertyChanged
    {
        private DateTime currentDate;
        private List<string> listOfIncome;
        private List<string> listOfSpending;
        private List<string> listOfExpenses;
        private List<Reimbursement> reimbursements;
        private List<Transaction> transactions;
        private List<Transaction> currentWeekTransactions;
        private Transaction firstItemClicked;
        private Transaction secondItemClicked;
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
        public event Action<object, Transaction, Transaction> OpenReimbursementWindow_Add;
        public event EventHandler<Transaction> OpenReimbursementWindow_Remove;
        public event EventHandler<RoutedEventArgs> ViewReimbursementsWindow;
        public event EventHandler<RoutedEventArgs> UploadFile;
        public event EventHandler<RoutedEventArgs> ClearData;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Homepage()
        {
            InitializeComponent();

            DataContext = this;
            transactions = FileManagement.ReadTransactions();

            if (transactions is null)
            {
                throw new NullReferenceException("Failed to load transactions data.");
            }

            if (transactions.Count == 0)
            {
                NoResultsText.Visibility = Visibility.Visible;
                return;
            }

            currentDate = transactions.Max(transaction => transaction.date);
            LoadPage();
            UpdateUI();
        }

        public void LoadPage()
        {
            GetData();
            UpdateUI();
        }

        private void GetData()
        {
            transactions = FileManagement.ReadTransactions();
            reimbursements = FileManagement.ReadReimbursements();
            listOfIncome = FileManagement.ReadList(TransItemType.Income);
            listOfSpending = FileManagement.ReadList(TransItemType.Spending);
            listOfExpenses = FileManagement.ReadList(TransItemType.Expenses);

            groupedTransactions = transactions.GroupBy(transaction => GetWeekNumber(transaction.date));
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
            if (groupedTransactions is null)
            {
                throw new NullReferenceException("Grouped transactions is null.");
            }

            var currentWeekGroup = groupedTransactions.FirstOrDefault(group => group.Key == GetWeekNumber(currentDate));

            if (currentWeekGroup is null)
            {
                throw new Exception("Current week group not found.");
            }

            currentWeekTransactions = currentWeekGroup.ToList();
        }

        private void UpdateTransactionGrid()
        {
            TransactionGrid.ItemsSource = currentWeekTransactions;
        }

        private void UpdateSummary()
        {
            if (currentWeekTransactions is null)
            {
                throw new NullReferenceException("Current week transactions is null.");
            }

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

        private List<string> GetListForType(TransItemType type)
        {
            switch (type)
            {
                case TransItemType.Income:
                    return listOfIncome;
                case TransItemType.Spending:
                    return listOfSpending;
                case TransItemType.Expenses:
                    return listOfExpenses;
                default:
                    throw new ArgumentException($"Invalid transaction type: {type}");
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

            List<Transaction> matchingTransactions = transactions
                .Where(transaction => items.Any(item => transaction.payee.Contains(item)))
                .ToList();

            float sum = matchingTransactions.Sum(transaction => transaction.amount);

            // Subtract the amounts of negative reimbursement transactions
            foreach (Reimbursement reimbursement in reimbursements)
            {
                sum -= reimbursement.ExcludeFromTotal(matchingTransactions);
            }

            return sum;
        }

        private void ForwardButtonClick(object sender, RoutedEventArgs e)
        {
            if (transactions.Count is 0) 
            {
                MessageBox.Show("No transactions found for the selected week.", "No Transactions");
                return;
            }
            DateTime newDate = currentDate.AddDays(7);
            firstItemClicked = null;
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
            if (transactions.Count is 0)
            {
                MessageBox.Show("No transactions found for the selected week.", "No Transactions");
                return;
            }
            DateTime newDate = currentDate.AddDays(-7);
            firstItemClicked = null;
            bool hasTransactions = transactions.Any(transaction => transaction.date.Date == newDate.Date);

            if (!hasTransactions)
            {
                MessageBox.Show("No transactions found for the selected week.", "No Transactions");
                return;
            }

            currentDate = newDate;
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
            ViewReimbursementsWindow?.Invoke(sender, e);
        }

        private void UploadFileButtonClick(object sender, RoutedEventArgs e)
        {
            UploadFile?.Invoke(sender, e);
        }

        private void ViewListClick(object sender, RoutedEventArgs e)
        {
            var button = (sender as Button);
            TransItemType type;
            switch (button.Tag)
            {
                case "Income":
                    type = TransItemType.Income;
                    break;
                case "Spending":
                    type = TransItemType.Spending;
                    break;
                case "Expenses":
                    type = TransItemType.Expenses;
                    break;
                default:
                    throw new ArgumentException("Invalid button tag.");
            }
            var list = GetListForType(type);
            OpenViewListWindow?.Invoke(sender, list, type);
        }

        private void TransactionGridItemClick(object sender, RoutedEventArgs e)
        {
            Transaction selectedItem = (sender as FrameworkElement)?.DataContext as Transaction;

            if (selectedItem is null)
            {
                throw new NullReferenceException("Selected item is null.");
            }

            if (selectedItem.IsReimbursement)
            {
                OpenReimbursementWindow_Remove?.Invoke(sender, selectedItem);
                return;
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
                selectedItem.IsItemClicked = true;
                secondItemClicked = selectedItem;
                bool isValidReimbursement = (secondItemClicked.amount > 0 && firstItemClicked.amount < 0) ||
                    (secondItemClicked.amount < 0 && firstItemClicked.amount > 0);

                if (!isValidReimbursement)
                {
                    MessageBox.Show("One transaction needs to be positive, and the other needs to be negative.");
                    selectedItem.IsItemClicked = false;
                    secondItemClicked = null;
                    return;
                }

                OpenReimbursementWindow_Add?.Invoke(sender, firstItemClicked, secondItemClicked);
                firstItemClicked.IsItemClicked = false;
                secondItemClicked.IsItemClicked = false;
                firstItemClicked = null;
                secondItemClicked = null;
            }
        }
    }
}