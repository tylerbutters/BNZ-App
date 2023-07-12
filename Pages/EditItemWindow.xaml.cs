using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for ReimbursementWindow.xaml
    /// </summary>
    public partial class EditItemWindow : Page
    {
        public event Action<object, ListItem, ListItem> GoBack;
        private ListItem oldItem;
        private ListItem newItem;
        public EditItemWindow(ListItem item)
        {
            InitializeComponent();

            if(item is null)
            {
                throw new ArgumentNullException(nameof(item), "item cannot be null");
            }

            DropdownBox.Text = item.category;
            NameInput.Text = item.formattedName;
            oldItem = item;
            newItem = item;
        }

        private void ConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            if(DropdownBox.SelectedItem != null)
            {
                newItem.category = (DropdownBox.SelectedItem as ComboBoxItem).Content.ToString();
            }
            if (string.IsNullOrEmpty(NameInput.Text))
            {
                MessageBox.Show("Name cannot be empty\nIf you want to delete item, go back.");
            }

            newItem.name = NameInput.Text;

            if (newItem.category is null || newItem.name is null)
            {
                throw new NullReferenceException("item is null");
            }

            GoBack?.Invoke(sender, oldItem, newItem);
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            GoBack?.Invoke(sender, oldItem, null);
        }
    }
}
