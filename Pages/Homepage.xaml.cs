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
            GetData();
            UpdateUI();
        }

        private void GetData()
        {
            transactions = FileManagement.ReadTransactions();
            reimbursements = FileManagement.ReadReimbursements();
            listOfItems = FileManagement.ReadList();

            groupedTransactions = transactions.GroupBy(transaction => GetWeekNumber(transaction.Date));
        }

        private void UpdateUI()
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
            UpdateSummary();
            if (pageType is PageType.Records)
            {
                TransactionGridPage.UpdateTransactionGrid(currentWeekTransactions);
            }
            else if (pageType is PageType.Details)
            {
                DetailsPage.UpdateDetails(currentWeekTransactions, listOfItems);
            }
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

        private void UpdateSummary()
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

            if (matchingTransactions.Count is 0)
            {
                return 0;
            }

            if (itemType == ListType.Income)
            {
                return matchingTransactions.Sum(transaction => transaction.Amount);
            }
            else
            {
                return CalculateSum(matchingTransactions);
            }
        }

        private List<Transaction> GetMatchingTransactions(List<Transaction> transactions, List<ListItem> items, ListType itemType)
        {
            if (transactions is null || items is null)
            {
                throw new ArgumentNullException("transactions or items is null");
            }

            List<Transaction> matchingTransactions = new List<Transaction>();

            foreach (Transaction transaction in transactions)
            {
                foreach (ListItem item in items)
                {
                    if (item.ListType == itemType)
                    {
                        bool isMatch = false;

                        switch (item.Category)
                        {
                            case "payee":
                                isMatch = transaction.Payee.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0;
                                break;
                            case "particulars":
                                isMatch = transaction.Particulars.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0;
                                break;
                            case "code":
                                isMatch = transaction.Code.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0;
                                break;
                            case "reference":
                                isMatch = transaction.Reference.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0;
                                break;
                            default:
                                throw new ArgumentException("Item type is not valid", nameof(item.Category));
                        }

                        if (isMatch && !matchingTransactions.Contains(transaction))
                        {
                            matchingTransactions.Add(transaction);
                        }
                    }
                }
            }

            return matchingTransactions;
        }

        private decimal CalculateSum(List<Transaction> transactions)
        {
            decimal sum = transactions.Sum(transaction => transaction.Amount);

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
            if (transactions.Count is 0)
            {
                MessageBox.Show("No transactions found for the selected week.", "No Transactions");
                return;
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

        private void SwitchPage()
        {
            TimeSpan duration = TimeSpan.FromSeconds(0.5);
            QuarticEase easing = new QuarticEase();
            Thickness boxMargin;
            DoubleAnimation rightAnim;
            DoubleAnimation leftAnim;
            double containerWidth = ActualWidth;

            
            if (pageType is PageType.Records)
            {
                boxMargin = new Thickness(-250, 0, 0, 0);
                LeftFrame.RenderTransform = new TranslateTransform(-containerWidth, 0);
                RightFrame.RenderTransform = new TranslateTransform(0, 0);
                leftAnim = CreateAnimation(-containerWidth, 0, duration);
                rightAnim = CreateAnimation(0, containerWidth, duration);
            }
            else if (pageType is PageType.Details)
            {
                boxMargin = new Thickness(250, 0, 0, 0);
                LeftFrame.RenderTransform = new TranslateTransform(0, 0);
                RightFrame.RenderTransform = new TranslateTransform(containerWidth, 0);
                leftAnim = CreateAnimation(0, -containerWidth, duration);
                rightAnim = CreateAnimation(containerWidth, 0, duration);
            }
            else
            {
                return;
            }

            ThicknessAnimation boxAnim = new ThicknessAnimation();
            boxAnim.Duration = duration;
            boxAnim.To = boxMargin;
            boxAnim.EasingFunction = easing;

            LeftFrame.RenderTransform.BeginAnimation(TranslateTransform.XProperty, leftAnim);
            RightFrame.RenderTransform.BeginAnimation(TranslateTransform.XProperty, rightAnim);

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(boxAnim);
            Storyboard.SetTarget(boxAnim, SelectionBox);
            Storyboard.SetTargetProperty(boxAnim, new PropertyPath("Margin"));
            storyboard.Begin();
        }

        private DoubleAnimation CreateAnimation(double from, double to, TimeSpan duration)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = from;
            animation.To = to;
            animation.Duration = duration;
            animation.EasingFunction = new QuarticEase();

            return animation;
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
    }
}