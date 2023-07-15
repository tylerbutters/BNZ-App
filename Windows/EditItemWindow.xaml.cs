using System;
using System.Windows;
using System.Windows.Controls;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for ReimbursementWindow.xaml
    /// </summary>
    public partial class EditItemWindow : Page
    {
        public event Action<object, ListItem, ListItem> GoBack;
        public event EventHandler<bool> GoBackHome;
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

            GoBack?.Invoke(sender, oldItem, newItem);
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            GoBack?.Invoke(sender, oldItem, null);
        }
    }
}
