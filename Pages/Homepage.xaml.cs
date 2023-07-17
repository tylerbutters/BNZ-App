using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for Homepage.xaml
    /// </summary>
    public partial class Homepage : Page
    {
        private enum PageType { Records, Details }

        private PageType pageType;
        public static decimal TaxPercentage => decimal.Parse(FileManagement.ReadProfile()[1]);
        public static decimal TaxTotal;
        public string FormattedTotalIncome { get => totalIncome.ToString("C"); set => totalIncome = decimal.Parse(value); }
        public string FormattedTotalSpending { get => totalSpending.ToString("C"); set => totalSpending = decimal.Parse(value); }
        public string FormattedTotalExpenses { get => totalExpenses.ToString("C"); set => totalExpenses = decimal.Parse(value); }
        public string FormattedTotalDecrease { get => totalDecrease.ToString("C"); set => totalDecrease = decimal.Parse(value); }
        public string FormattedTotal { get => total.ToString("C"); set => total = decimal.Parse(value); }
        public bool TotalIsNegative => total < 0;

        private decimal totalIncome;
        private decimal totalSpending;
        private decimal totalExpenses;
        private decimal totalDecrease;
        private decimal total;

        private DateTime currentDate;
        private DateTime latestDate;
        private List<ListItem> listOfItems;
        private List<Reimbursement> reimbursements;
        private List<Transaction> transactions = FileManagement.ReadTransactions();
        public TransactionGridPage TransactionGridPage = new TransactionGridPage();
        public DetailsPage DetailsPage = new DetailsPage();
        private List<Transaction> currentWeekTransactions;
        private IEnumerable<IGrouping<int, Transaction>> groupedTransactions;

        public event Action<List<ListItem>, ListType> OpenListWindow;
        public Homepage()
        {
            InitializeComponent();

            DataContext = this;

            LeftFrame.Content = TransactionGridPage;
            RightFrame.Content = DetailsPage;
            pageType = PageType.Records;
            SwitchPage();
            latestDate = transactions.Max(transaction => transaction.Date);
            currentDate = latestDate;
            LoadPage();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                RightFrame.RenderTransform = new TranslateTransform(ActualWidth, 0);
            }), DispatcherPriority.Loaded);
        }


        public void LoadPage()
        {
            UpdateData();
            UpdateUI();
        }

        private void UpdateData()
        {
            transactions = FileManagement.ReadTransactions();
            reimbursements = FileManagement.ReadReimbursements();
            listOfItems = FileManagement.ReadList();

            groupedTransactions = transactions.GroupBy(transaction => GetWeekNumber(transaction.Date));
        }

        private void UpdateUI()
        {
            GetNewWeekTransactions();
            UpdateDateDisplay();
            UpdateTotalSummary();

            if (currentDate == latestDate)
            {
                LatestButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                LatestButton.Visibility = Visibility.Visible;
            }

            switch (pageType)
            {
                case PageType.Records:
                    TransactionGridPage.UpdateTransactionGrid(currentWeekTransactions);
                    break;
                case PageType.Details:
                    DetailsPage.UpdateDetails(currentWeekTransactions, listOfItems);
                    break;
            }
        }

        private void UpdateDateDisplay()
        {
            string startDate = GetStartDateOfWeek(currentDate).ToString("dd/MM/yy");
            string endDate = DateTime.Parse(startDate).AddDays(7).ToString("dd/MM/yy");
            string month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(currentDate.Month);
            string weekNumber = GetWeekNumber(currentDate).ToString();
            WeekAndMonth.Text = month + ", Week " + weekNumber;
            Dates.Text = startDate + " - " + endDate;
        }

        private void UpdateTotalSummary()
        {
            if (currentWeekTransactions is null)
            {
                throw new NullReferenceException("Current week transactions is null.");
            }

            totalIncome = GetTotal(currentWeekTransactions, listOfItems, ListType.Income);
            TaxTotal = totalIncome * TaxPercentage;
            totalSpending = GetTotal(currentWeekTransactions, listOfItems, ListType.Spending);
            totalExpenses = GetTotal(currentWeekTransactions, listOfItems, ListType.Expenses) - TaxTotal;
            total = totalIncome + totalSpending + totalExpenses;

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

        private decimal GetTotal(List<Transaction> transactions, List<ListItem> items, ListType itemType)
        {
            if (items is null)
            {
                return 0;
            }

            List<Transaction> matchingTransactions = GetMatchingTransactions(transactions, items, itemType);
            decimal sum = matchingTransactions.Sum(transaction => transaction.Amount);
            decimal excludedAmount = ExcludeReimbursements(matchingTransactions);
            decimal total = sum - excludedAmount;

            return total;
        }

        private bool IsMatch(Transaction transaction, ListItem item)
        {
            switch (item.Category)
            {
                case "payee":
                    return IsStringMatch(transaction.Payee, item.Name);
                case "particulars":
                    return IsStringMatch(transaction.Particulars, item.Name);
                case "code":
                    return IsStringMatch(transaction.Code, item.Name);
                case "reference":
                    return IsStringMatch(transaction.Reference, item.Name);
                default:
                    throw new ArgumentException("Item type is not valid", nameof(item.Category));
            }
        }

        private bool IsStringMatch(string source, string target)
        {
            return source.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private List<Transaction> GetMatchingTransactions(List<Transaction> transactions, List<ListItem> items, ListType itemType)
        {
            if (transactions is null || items is null)
            {
                throw new ArgumentNullException("transactions or items is null");
            }

            List<ListItem> itemsOfType = items.Where(item => item.ListType == itemType).ToList();

            List<Transaction> matchingTransactions = transactions
                .Where(transaction => itemsOfType
                    .Any(item => IsMatch(transaction, item)))
                .Distinct()
                .ToList();

            return matchingTransactions;
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

        private void GetNewWeekTransactions()
        {
            if (groupedTransactions is null)
            {
                throw new NullReferenceException("Grouped transactions is null.");
            }

            List<Transaction> newWeekTransactions = groupedTransactions.FirstOrDefault(group => group.Key == GetWeekNumber(currentDate)).ToList();

            if (newWeekTransactions is null || newWeekTransactions.Count == 0)
            {
                throw new NullReferenceException("Current week group not found.");
            }

            currentWeekTransactions = newWeekTransactions;
        }

        private void ForwardButtonClick(object sender, RoutedEventArgs e)
        {
            if (transactions is null)
            {
                throw new NullReferenceException("Transactions is null");
            }
            DateTime newDate = currentDate.AddDays(7);

            bool hasTransactions = groupedTransactions.Any(group => group.Key == GetWeekNumber(newDate));

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
            if (transactions is null)
            {
                throw new NullReferenceException("Transactions is null");
            }
            DateTime newDate = currentDate.AddDays(-7);

            bool hasTransactions = groupedTransactions.Any(group => group.Key == GetWeekNumber(newDate));

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
            OpenListWindow?.Invoke(listOfItems, type);
        }

        private void DetailsButtonClick(object sender, RoutedEventArgs e)
        {
            if (pageType != PageType.Details)
            {
                pageType = PageType.Details;
                SwitchPage();
                UpdateUI();
            }
        }

        private void RecordsButtonClick(object sender, RoutedEventArgs e)
        {
            if (pageType != PageType.Records)
            {
                pageType = PageType.Records;
                SwitchPage();
                UpdateUI();
            }
        }

        private void SwitchPage()
        {
            TimeSpan duration = TimeSpan.FromSeconds(0.5);
            QuarticEase easing = new QuarticEase();
            ThicknessAnimation selectionBoxAnimation = null;
            DoubleAnimation rightFrameAnimation = null;
            DoubleAnimation leftFrameAnimation = null;

            switch (pageType)
            {
                case PageType.Records:                   
                    selectionBoxAnimation = new ThicknessAnimation { To = new Thickness(-250, 0, 0, 0), Duration = duration, EasingFunction = easing };
                    leftFrameAnimation = new DoubleAnimation { From = -ActualWidth, To = 0, Duration = duration, EasingFunction = easing };
                    rightFrameAnimation = new DoubleAnimation { From = 0, To = ActualWidth, Duration = duration, EasingFunction = easing };
                    LeftFrame.RenderTransform = new TranslateTransform(-ActualWidth, 0);
                    RightFrame.RenderTransform = new TranslateTransform(0, 0);
                    break;

                case PageType.Details:                    
                    selectionBoxAnimation = new ThicknessAnimation { To = new Thickness(250, 0, 0, 0), Duration = duration, EasingFunction = easing };
                    leftFrameAnimation = new DoubleAnimation { From = 0, To = -ActualWidth, Duration = duration, EasingFunction = easing };
                    rightFrameAnimation = new DoubleAnimation { From = ActualWidth, To = 0, Duration = duration, EasingFunction = easing };
                    LeftFrame.RenderTransform = new TranslateTransform(0, 0);
                    RightFrame.RenderTransform = new TranslateTransform(ActualWidth, 0);
                    break;
            }

            LeftFrame.RenderTransform.BeginAnimation(TranslateTransform.XProperty, leftFrameAnimation);
            RightFrame.RenderTransform.BeginAnimation(TranslateTransform.XProperty, rightFrameAnimation);

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(selectionBoxAnimation);
            Storyboard.SetTarget(selectionBoxAnimation, SelectionBox);
            Storyboard.SetTargetProperty(selectionBoxAnimation, new PropertyPath("Margin"));
            storyboard.Begin();
        }
    }
}