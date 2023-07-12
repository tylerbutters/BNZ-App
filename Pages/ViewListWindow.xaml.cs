using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for ViewListWindow.xaml
    /// </summary>
    public partial class ViewListWindow : Page
    {
        public event EventHandler<bool> GoBack;
        public event EventHandler<ListItem> OpenEditItemWindow;
        private bool isAddButtonClicked;
        private bool isDeleteButtonClicked;
        private List<ListItem> list;
        private List<ListItem> listOfType;
        private ListType listType;
        public ViewListWindow(List<ListItem> list, ListType listType)
        {
            InitializeComponent();

            if (list is null)
            {
                throw new ArgumentNullException(nameof(list), "The list parameter cannot be null.");
            }

            if (list.Count == 0)
            {
                Console.Write($"The list of {listType} is empty.");
            }

            this.list = list;
            this.listType = listType;
            listOfType = list.Where(item => item.listType == listType).ToList();

            switch (listType)
            {
                case ListType.Income:
                    Title.Text = "Transactions Classified as Income";
                    break;
                case ListType.Spending:
                    Title.Text = "Transactions Classified as Spending";
                    break;
                case ListType.Expenses:
                    Title.Text = "Transactions Classified as Expenses";
                    TaxInputBox.Visibility = Visibility.Visible;
                    TaxInput.Text = Homepage.taxPercentage * 100 + "%";
                    break;
            }

            ListGrid.ItemsSource = listOfType;
        }

        private void DoneButtonClick(object sender, RoutedEventArgs e)
        {
            if (listType is ListType.Expenses)
            {
                if (TaxInput.Text is "")
                {
                    MessageBox.Show("Please enter tax amount");
                    return;
                }
                Homepage.taxPercentage = float.Parse(TaxInput.Text = TaxInput.Text.Substring(0, TaxInput.Text.Length - 1))/100;
            }
            if (isAddButtonClicked || isDeleteButtonClicked) //items have been edited
            {
                FileManagement.WriteList(list);
            }

            GoBack?.Invoke(sender, isAddButtonClicked || isDeleteButtonClicked);
        }

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {

            string newItemName = null;
            string newItemType = null;
            if (NameInput.Text != "Name")
            {
                newItemName = NameInput.Text;
            }
            if (DropdownBox.SelectedItem != null)
            {
                newItemType = (DropdownBox.SelectedItem as ComboBoxItem).Content.ToString();
            }

            if (string.IsNullOrWhiteSpace(newItemName) || newItemType is null)
            {
                MessageBox.Show("Please enter the name and the type.");
                return;
            }

            list.Add(new ListItem(listType, newItemType, newItemName));
            listOfType.Add(new ListItem(listType, newItemType, newItemName));
            ListGrid.ItemsSource = null;
            ListGrid.ItemsSource = listOfType;
            isAddButtonClicked = true;
            ResetInput();
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            ListItem item = (sender as Button)?.DataContext as ListItem;
            if (item is null)
            {
                throw new NullReferenceException("item cannot be null");
            }

            list.Remove(item);
            listOfType.Remove(item);
            ListGrid.ItemsSource = null;
            ListGrid.ItemsSource = listOfType;
            isDeleteButtonClicked = true;
        }

        private void NameInputGotFocus(object sender, RoutedEventArgs e)
        {
            NameInput.Text = "";
            NameInput.Foreground = Brushes.Black;
        }

        private void DropdownBoxGotFocus(object sender, SelectionChangedEventArgs e)
        {
            DropdownBox.Foreground = Brushes.Black;
        }

        private void ResetInput()
        {
            NameInput.Text = "Name";
            NameInput.Foreground = Brushes.Gray;
            DropdownBox.Text = "Category";
            DropdownBox.Foreground = Brushes.Gray;
        }

        private void ListGridItemClick(object sender, RoutedEventArgs e)
        {
            ListItem oldItem = (sender as FrameworkElement)?.DataContext as ListItem;
            EditItemWindow editItemWindow = new EditItemWindow(oldItem);
            EditPopup.Content = editItemWindow;
            editItemWindow.GoBack += CloseEditItemWindow;
            
        }
        private void CloseEditItemWindow(object sender, ListItem oldItem, ListItem newItem)
        {
            EditPopup.Content = null;

            if (newItem is null)
            {
                return;
            }
            if(oldItem is null)
            {
                throw new ArgumentNullException(nameof(oldItem), "old item cannot be null");
            }
            list.Remove(oldItem);
            listOfType.Remove(oldItem);
            list.Add(newItem);
            listOfType.Add(newItem);
            ListGrid.ItemsSource = null;
            ListGrid.ItemsSource = listOfType;
            isAddButtonClicked = true;
        }
        private void TaxInputGotFocus(object sender, RoutedEventArgs e)
        {
            TaxInput.Text = TaxInput.Text.Substring(0, TaxInput.Text.Length - 1);
        }

        private void TaxInputLostFocus(object sender, RoutedEventArgs e)
        {
            TaxInput.Text += "%";
        }
    }
}