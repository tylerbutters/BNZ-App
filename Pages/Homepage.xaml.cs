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
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public static float taxPercentage = 0.105f;
        public string formattedTotalIncome { get => totalIncome.ToString("C"); set { totalIncome = float.Parse(value); OnPropertyChanged(nameof(totalIncome)); } }
        public string formattedTotalSpending { get => totalSpending.ToString("C"); set { totalSpending = float.Parse(value); OnPropertyChanged(nameof(totalSpending)); } }
        public string formattedTotalExpenses { get => totalExpenses.ToString("C"); set { totalExpenses = float.Parse(value); OnPropertyChanged(nameof(totalExpenses)); } }
        public string formattedTotalDecrease { get => totalDecrease.ToString("C"); set { totalDecrease = float.Parse(value); OnPropertyChanged(nameof(totalDecrease)); } }
        public string formattedTotal { get => total.ToString("C"); set { total = float.Parse(value); OnPropertyChanged(nameof(total)); } }
        private bool TotalIsNegative;
        public bool totalIsNegative { get => total < 0; set { TotalIsNegative = value; OnPropertyChanged(nameof(TotalIsNegative)); } }
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
        private Transaction firstItemClicked;
        private Transaction secondItemClicked;
        private IEnumerable<IGrouping<int, Transaction>> groupedTransactions;

        public event Action<object, List<ListItem>, ListType> OpenViewListWindow;
        public event Action<object, Transaction, Transaction> OpenReimbursementWindow_Add;
        public event EventHandler<Transaction> OpenReimbursementWindow_Remove;
        public event EventHandler<RoutedEventArgs> ViewReimbursementsWindow;
        public event EventHandler<RoutedEventArgs> UploadFile;
        public event EventHandler<RoutedEventArgs> ClearData;
        public Homepage()
        {
            InitializeComponent();

            DataContext = this;

            if (transactions is null)
            {
                throw new NullReferenceException("Failed to load transactions data.");
            }

            if (transactions.Count == 0)
            {
                MessageBox.Show("No transactions in file.\nFeatures will be limited.");
                NoResultsText.Visibility = Visibility.Visible;
                return;
            }
            latestDate = transactions.Max(transaction => transaction.date);
            currentDate = latestDate;
            LoadPage();
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
            listOfItems = FileManagement.ReadList();

            groupedTransactions = transactions.GroupBy(transaction => GetWeekNumber(transaction.date));
        }

        private void UpdateUI()
        {
            if(currentDate == latestDate)
            {
                LatestButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                LatestButton.Visibility = Visibility.Visible;
            }

            UpdateWeekNumberDisplay();
            UpdateCurrentWeekGroup();
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

            totalIncome = GetTotal(currentWeekTransactions, listOfItems, ListType.Income);
            totalSpending = GetTotal(currentWeekTransactions, listOfItems, ListType.Spending);
            totalExpenses = GetTotal(currentWeekTransactions, listOfItems, ListType.Expenses) + (totalIncome * taxPercentage);
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

            return CalculateSum(matchingTransactions);
        }

        private List<Transaction> GetMatchingTransactions(List<Transaction> transactions, List<ListItem> items, ListType itemType)
        {
            List<Transaction> matchingTransactions = new List<Transaction>();
            foreach (ListItem item in items)
            {
                if (item.listType == itemType)
                {
                    switch (item.category.ToLower())
                    {
                        case "payee":
                            matchingTransactions.AddRange(transactions.Where(transaction => transaction.payee.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0));
                            break;
                        case "particulars":
                            matchingTransactions.AddRange(transactions.Where(transaction => transaction.particulars.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0));
                            break;
                        case "code":
                            matchingTransactions.AddRange(transactions.Where(transaction => transaction.code.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0));
                            break;
                        case "reference":
                            matchingTransactions.AddRange(transactions.Where(transaction => transaction.reference.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0));
                            break;
                        default:
                            throw new ArgumentException("Item type is not valid", nameof(item.category));
                    }
                }
            }

            return matchingTransactions;
        }
        private float CalculateSum(List<Transaction> transactions)
        {
            float sum = transactions.Sum(transaction => transaction.amount);

            foreach (Reimbursement reimbursement in reimbursements)
            {
                sum -= reimbursement.ExcludeFromTotal(transactions);
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
            ViewReimbursementsWindow?.Invoke(sender, e);
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
            OpenViewListWindow?.Invoke(sender, listOfItems, type);
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