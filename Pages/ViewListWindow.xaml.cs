using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for ViewListWindow.xaml
    /// </summary>
    public partial class ViewListWindow : Page
    {
        public event EventHandler<bool> GoBack;
        private bool isAddButtonClicked;
        private bool isDeleteButtonClicked;
        private List<string> list;
        private TransItemType type;
        public ViewListWindow(List<string> list, TransItemType type)
        {
            InitializeComponent();

            if (list is null)
            {
                throw new ArgumentNullException(nameof(list), "The list parameter cannot be null.");
            }

            if (list.Count == 0)
            {
                Console.Write($"The list of {type.ToString()} is empty.");
            }

            this.list = list;
            this.type = type;

            switch (type)
            {
                case TransItemType.Income:
                    Title.Text = "Transactions Classified as Income";
                    break;
                case TransItemType.Spending:
                    Title.Text = "Transactions Classified as Spending";
                    break;
                case TransItemType.Expenses:
                    Title.Text = "Transactions Classified as Expenses";
                    break;
            }

            ListGrid.ItemsSource = list;
        }

        private void DoneButtonClick(object sender, RoutedEventArgs e)
        {
            if (isAddButtonClicked || isDeleteButtonClicked) //items have been edited
            {
                FileManagement.WriteList(list, type);
            }

            GoBack?.Invoke(sender, isAddButtonClicked || isDeleteButtonClicked);
        }

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            string newItem = NewItemInput.Text;
            if (!string.IsNullOrWhiteSpace(newItem))
            {
                list.Add(newItem);
                ListGrid.ItemsSource = null;
                ListGrid.ItemsSource = list;
                isAddButtonClicked = true;
            }
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            string item = (sender as Button)?.DataContext as string;
            if (!string.IsNullOrWhiteSpace(item))
            {
                list.Remove(item);
                ListGrid.ItemsSource = null;
                ListGrid.ItemsSource = list;
                isDeleteButtonClicked = true;
            }
        }

    }
}
