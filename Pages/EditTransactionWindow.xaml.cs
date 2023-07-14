using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for EditTransactionWindow.xaml
    /// </summary>
    public partial class EditTransactionWindow : Page
    {
        public event EventHandler<bool> GoBack;
        public event EventHandler<Transaction> ReturnTransaction;
        public event EventHandler<Transaction> OpenRemoveReimbursementWindow;
        public event Action<object, Transaction, Transaction> OpenAddReimbursementWindow;
        private Button selectedButton;
        private DataGridCell selectedCell;
        private Transaction transaction;
        private Transaction transaction1;
        private Transaction transaction2;
        private ListItem item;
        private List<ListItem> list = FileManagement.ReadList();
        private ListType type { get { return (ListType)Enum.Parse(typeof(ListType), selectedButton.Tag.ToString()); } }
        private string category { get { return selectedCell.Column.Header.ToString(); } }
        private string name { get { return (selectedCell.Content as TextBlock).Text; } }

        public EditTransactionWindow(Transaction transaction1, Transaction transaction2)
        {
            InitializeComponent();

            if (transaction1 is null)
            {
                throw new ArgumentNullException(nameof(transaction1), "Transaction cannot be null");
            }
            if (transaction2 != null)
            {
                this.transaction1 = transaction1;
                this.transaction2 = transaction2;
            }

            transaction = transaction1;
            TransactionGrid.ItemsSource = new List<Transaction> { transaction };

            UpdateDefaults();
        }

        public void UpdateDefaults()
        {
            if (transaction.isHighlighted)
            {
                ButtonsWithDelete.Visibility = Visibility.Visible;
                ButtonsWithoutDelete.Visibility = Visibility.Collapsed;

                UpdateButtons();
                UpdateCells();
            }

            if (transaction.isReimbursement || transaction.wantToBeReimbursement)
            {
                ReimbursementButton.Background = Brushes.DarkRed;
            }
        }

        private void UpdateButtons()
        {
            if (transaction.isIncome)
            {
                IncomeButton.Background = (Brush)new BrushConverter().ConvertFrom("#33639E");
                selectedButton = IncomeButton;
            }
            else if (transaction.isSpending)
            {
                SpendingButton.Background = (Brush)new BrushConverter().ConvertFrom("#33639E");
                selectedButton = SpendingButton;
            }
            else if (transaction.isExpense)
            {
                ExpensesButton.Background = (Brush)new BrushConverter().ConvertFrom("#33639E");
                selectedButton = ExpensesButton;
            }
        }

        private void UpdateCells()
        {
            foreach (ListItem item in list)
            {
                string name = item.name.ToLowerInvariant();
                string payee = transaction.payee.ToLowerInvariant();
                string particulars = transaction.particulars.ToLowerInvariant();
                string code = transaction.code.ToLowerInvariant();
                string reference = transaction.reference.ToLowerInvariant();

                if (payee.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    UpdateDefaultCellStyle(2);
                    this.item = item;
                }
                else if (particulars.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    UpdateDefaultCellStyle(3);
                    this.item = item;
                }
                else if (code.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    UpdateDefaultCellStyle(4);
                    this.item = item;
                }
                else if (reference.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    UpdateDefaultCellStyle(5);
                    this.item = item;
                }
            }
        }
        private void UpdateDefaultCellStyle(int index)
        {
            TransactionGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                var cell = TransactionGrid.Columns[index].GetCellContent(TransactionGrid.Items[0])?.Parent as DataGridCell;
                if (cell != null)
                {
                    cell.Background = (Brush)new BrushConverter().ConvertFrom("#33639E");
                    cell.Foreground = Brushes.White;
                    cell.FontWeight = FontWeights.Black;
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void UpdateCellStyle(DataGridCell selectedCell, DataGridCell unselectedCell)
        {
            if (selectedCell != null)
            {
                selectedCell.Background = (Brush)new BrushConverter().ConvertFrom("#33639E");
                selectedCell.Foreground = Brushes.White;
                selectedCell.FontWeight = FontWeights.Black;
            }
            if (unselectedCell != null)
            {
                unselectedCell.Background = Brushes.White;
                unselectedCell.Foreground = Brushes.Black;
                unselectedCell.FontWeight = FontWeights.Normal;
            }
        }

        private void UpdateButtonStyle(Button selectedButton, Button unselectedButton)
        {
            if (selectedButton != null)
            {
                selectedButton.Background = (Brush)new BrushConverter().ConvertFrom("#33639E");
            }
            if (unselectedButton != null)
            {
                unselectedButton.Background = (Brush)new BrushConverter().ConvertFrom("#1F487E");
            }
        }

        private void TransactionGridClick(object sender, RoutedEventArgs e)
        {
            DataGridCell clickedCell = sender as DataGridCell;

            if (clickedCell is null)
            {
                throw new NullReferenceException(nameof(clickedCell));
            }

            if (clickedCell.Column.Header is "Date" || clickedCell.Column.Header is "Amount")
            {
                return;
            }
            else if (selectedCell is null)
            {
                UpdateCellStyle(clickedCell, null);
                selectedCell = clickedCell;
            }
            else if (clickedCell == selectedCell)
            {
                UpdateCellStyle(null, clickedCell);
                selectedCell = null;
            }
            else if (clickedCell != selectedCell)
            {
                UpdateCellStyle(clickedCell, selectedCell);
                selectedCell = clickedCell;
            }
        }

        private void TypeButtonClick(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if (clickedButton is null)
            {
                throw new NullReferenceException("Button is null");
            }

            if (selectedButton is null)
            {
                UpdateButtonStyle(clickedButton, null);
                selectedButton = clickedButton;
            }
            else if (clickedButton == selectedButton)
            {
                UpdateButtonStyle(null, clickedButton);
                selectedButton = null;
            }
            else if (clickedButton != selectedButton)
            {
                UpdateButtonStyle(clickedButton, selectedButton);
                selectedButton = clickedButton;
            }
        }

        private void ReimbursementButtonClick(object sender, RoutedEventArgs e)
        {
            if (transaction.isReimbursement)
            {
                OpenRemoveReimbursementWindow?.Invoke(sender, transaction);
            }
            else if (transaction.wantToBeReimbursement)
            {
                ReimbursementButton.Background = (Brush)new BrushConverter().ConvertFrom("#33639E");
                transaction.wantToBeReimbursement = false;
            }
            else if (!transaction.wantToBeReimbursement)
            {
                if (transaction2 is null)
                {
                    transaction.wantToBeReimbursement = true;
                    ReturnTransaction(sender, transaction);
                    GoBack?.Invoke(sender, false);
                }
                else
                {
                    bool isValidReimbursement = (transaction1.amount > 0 && transaction2.amount < 0) ||
                        (transaction1.amount < 0 && transaction2.amount > 0);

                    if (!isValidReimbursement)
                    {
                        MessageBox.Show("One transaction needs to be positive, and the other needs to be negative.");
                        return;
                    }

                    OpenAddReimbursementWindow?.Invoke(sender, transaction1, transaction2);
                }
            }
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            list.Remove(item);
            FileManagement.WriteList(list);
            GoBack?.Invoke(sender, true);
        }

        private void ConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            if (selectedCell != null || selectedButton != null)
            {
                List<ListItem> list = FileManagement.ReadList();
                list.Add(new ListItem(type, category, name));
                FileManagement.WriteList(list);
            }
            GoBack?.Invoke(sender, true);
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            GoBack?.Invoke(sender, false);
        }
    }
}
