using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        public string FormattedTotalIncome
        {
            get { return formattedTotalIncome; }
            set
            {
                formattedTotalIncome = value;
                OnPropertyChanged(nameof(FormattedTotalIncome));
            }
        }
        public string FormattedTotalSpending
        {
            get { return formattedTotalSpending; }
            set
            {
                formattedTotalSpending = value;
                OnPropertyChanged(nameof(FormattedTotalSpending));
            }
        }
        public string FormattedTotalExpenses
        {
            get { return formattedTotalExpenses; }
            set
            {
                formattedTotalExpenses = value;
                OnPropertyChanged(nameof(FormattedTotalExpenses));
            }
        }
        public string FormattedTotalIncrease
        {
            get { return formattedTotalIncrease; }
            set
            {
                formattedTotalIncrease = value;
                OnPropertyChanged(nameof(FormattedTotalIncrease));
            }
        }
        public string FormattedTotalDecrease
        {
            get { return formattedTotalDecrease; }
            set
            {
                formattedTotalDecrease = value;
                OnPropertyChanged(nameof(FormattedTotalDecrease));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Homepage()
        {
            InitializeComponent();

            DataContext = this;

            GetData();

            UpdateUI();
        }
        private void GetData()
        {
            transactions = FileManagement.ReadTransactions();
            listOfIncome = FileManagement.ReadListOfIncome();
            listOfSpending = FileManagement.ReadListOfSpending();
            listOfExpenses = FileManagement.ReadListOfExpenses();

            currentDate = transactions.Max(transaction => transaction.date);

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
            return matchingTransactions.Sum(transaction => transaction.amount);
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
                totalExpenses = GetTotal(currentWeekTransactions, listOfExpenses);
                totalIncrease = totalIncome - (totalIncome / 10) + totalSpending + totalExpenses;
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

        }

        private void ViewSpendingClick(object sender, RoutedEventArgs e)
        {
            //PopUpWindow.Content = new ViewSpendingWindow(listOfSpending);
        }

        private void ViewExpensesClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
