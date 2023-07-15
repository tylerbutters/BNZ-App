using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for Homepage.xaml
    /// </summary>
    public partial class Homepage : Page
    {
        private enum PageType { Records, Details }
        private PageType pageType;
        public static double taxPercentage = 0.105;
        public static float taxTotal;
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
        public TransactionGridPage transactionGridPage = new TransactionGridPage();
        public DetailsPage detailsPage = new DetailsPage();
        private List<Transaction> currentWeekTransactions;
        private IEnumerable<IGrouping<int, Transaction>> groupedTransactions;

        public event Action<object, List<ListItem>, ListType> OpenListWindow;
        public Homepage()
        {
            InitializeComponent();

            DataContext = this;

            pageType = PageType.Records;
            Main.Content = transactionGridPage;
            MoveSelectionBox();
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
            UpdateSummary();
            if (pageType is PageType.Records)
            {
                transactionGridPage.UpdateTransactionGrid(currentWeekTransactions);
            }
            else if (pageType is PageType.Details)
            {
                detailsPage.UpdateDetails(currentWeekTransactions, listOfItems);
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
            taxTotal = (float)(totalIncome * taxPercentage);
            totalSpending = GetTotal(currentWeekTransactions, listOfItems, ListType.Spending);
            totalExpenses = GetTotal(currentWeekTransactions, listOfItems, ListType.Expenses) - taxTotal;
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
            if (transactions is null || items is null)
            {
                throw new ArgumentNullException("transactions or items is null");
            }

            List<Transaction> matchingTransactions = new List<Transaction>();

            foreach (Transaction transaction in transactions)
            {
                foreach (ListItem item in items)
                {
                    if (item.listType == itemType)
                    {
                        bool isMatch = false;

                        switch (item.category)
                        {
                            case "payee":
                                isMatch = transaction.payee.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0;
                                break;
                            case "particulars":
                                isMatch = transaction.particulars.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0;
                                break;
                            case "code":
                                isMatch = transaction.code.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0;
                                break;
                            case "reference":
                                isMatch = transaction.reference.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0;
                                break;
                            default:
                                throw new ArgumentException("Item type is not valid", nameof(item.category));
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

        private void MoveSelectionBox()
        {
            TimeSpan duration = TimeSpan.FromSeconds(0.3);
            Thickness toMargin;

            if (pageType == PageType.Details)
            {
                toMargin = new Thickness(250, 0, 0, 0);
            }
            else if (pageType == PageType.Records)
            {
                toMargin = new Thickness(-250, 0, 0, 0);
            }
            else
            {
                return;
            }

            ThicknessAnimation animation = new ThicknessAnimation();
            animation.Duration = duration;
            animation.To = toMargin;
            animation.EasingFunction = new CubicEase();
            animation.Completed += (sender, e) =>
            {
                SelectionBox.Margin = toMargin;
            };
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, SelectionBox);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Margin"));
            storyboard.Begin();
        }

        private void DetailsButtonClick(object sender, RoutedEventArgs e)
        {
            if (pageType != PageType.Details)
            {
                Main.Content = null;
                Main.Content = detailsPage;
                pageType = PageType.Details;
                MoveSelectionBox();
                UpdateUI();
            }
        }

        private void RecordsButtonClick(object sender, RoutedEventArgs e)
        {
            if (pageType != PageType.Records)
            {
                Main.Content = null;
                Main.Content = transactionGridPage;
                pageType = PageType.Records;
                MoveSelectionBox();
                UpdateUI();
            }
        }
    }
}