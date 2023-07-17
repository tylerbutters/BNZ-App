using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for ReimbursementWindow.xaml
    /// </summary>
    public partial class EditItemWindow : Page
    {
        public event Action<ListItem, ListItem> GoBack;
        private ListItem oldItem;
        private ListItem newItem;
        public EditItemWindow(ListItem item)
        {
            InitializeComponent();

            if(item is null)
            {
                throw new ArgumentNullException(nameof(item), "item cannot be null");
            }

            DropdownBox.Text = item.Category;
            NameInput.Text = item.FormattedName;
            oldItem = item;
            newItem = item;
        }

        private void BackgroundClick(object sender, MouseButtonEventArgs e)
        {
            CancelButtonClick(sender, e);
        }

        private void ConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            if(DropdownBox.SelectedItem != null)
            {
                newItem.Category = (DropdownBox.SelectedItem as ComboBoxItem).Content.ToString();
            }
            if (string.IsNullOrEmpty(NameInput.Text))
            {
                MessageBox.Show("Name cannot be empty\nIf you want to delete item, go back.");
            }

            newItem.Name = NameInput.Text;

            if (newItem.Category is null || newItem.Name is null)
            {
                throw new NullReferenceException("item is null");
            }

            GoBack?.Invoke(oldItem, newItem);
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            GoBack?.Invoke(oldItem, null);
        }

        private void NameInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmButtonClick(sender, e);
                Keyboard.ClearFocus();
            }
        }
    }
}
