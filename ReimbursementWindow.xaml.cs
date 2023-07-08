using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for ReimbursementWindow.xaml
    /// </summary>
    public partial class ReimbursementWindow : Page
    {
        public event EventHandler<RoutedEventArgs> GoBack;
        private List<Transaction> items;
        public ReimbursementWindow(Transaction _firstItem, Transaction _secondItem)
        {
            InitializeComponent();

            items = new List<Transaction>
            {
                _firstItem,
                _secondItem
            };

            ComparisonGrid.ItemsSource = items;
        }

        private void ConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            GoBack?.Invoke(sender, e);
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            GoBack?.Invoke(sender, e);
        }
    }
}
