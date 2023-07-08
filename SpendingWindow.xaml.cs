using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for SpendingWindow.xaml
    /// </summary>
    public partial class SpendingWindow : Page
    {
        public event EventHandler<RoutedEventArgs> GoBack;
        private bool isAddButtonClicked { get; set; }
        private bool isDeleteButtonClicked { get; set; }
        private List<string> listOfSpending { get; set; }
        public SpendingWindow(List<string> _listOfSpending)
        {
            InitializeComponent();

            if(_listOfSpending is null)
            {
                throw new NullReferenceException();
            }

            listOfSpending = _listOfSpending;

            if (listOfSpending.Count is 0 )
            {
                Console.Write("list of spending is empty");
            }

            SpendingGrid.ItemsSource = listOfSpending;
        }

        private void DoneButtonClick(object sender, RoutedEventArgs e)
        {
            if (isAddButtonClicked || isDeleteButtonClicked)
            {
                FileManagement.WriteListOfSpending(listOfSpending);
            }
            GoBack?.Invoke(sender,e);
        }
        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            isAddButtonClicked = true;
            listOfSpending.Add(NewItemInput.Text);
            SpendingGrid.ItemsSource = null;
            SpendingGrid.ItemsSource = listOfSpending;
        }
        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            isDeleteButtonClicked = true;
            string item = (sender as Button).DataContext as string;

            listOfSpending.Remove(item);
            SpendingGrid.ItemsSource = null;
            SpendingGrid.ItemsSource = listOfSpending;
        }

    }
}
