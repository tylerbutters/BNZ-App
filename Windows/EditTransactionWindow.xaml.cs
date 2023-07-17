using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for EditTransactionWindow.xaml
    /// </summary>
    public partial class EditTransactionWindow : Page
    {
        public event Action<bool> GoBack;
        public event Action<Transaction> ReturnTransaction;
        public event Action<Transaction> OpenRemoveReimbursementWindow;
        public event Action<Transaction, Transaction> OpenAddReimbursementWindow;
        private Button selectedButton;
        private DataGridCell selectedCell;
        private Transaction transaction;
        private Transaction transaction1;
        private Transaction transaction2;
        private ListItem item;
        private List<ListItem> list = FileManagement.ReadList();
        private ListType Type => (ListType)Enum.Parse(typeof(ListType), selectedButton.Tag.ToString());
        private string Category => selectedCell.Column.Header.ToString();
        private string Name => (selectedCell.Content as TextBlock).Text;

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

        private void BackgroundClick(object sender, MouseButtonEventArgs e)
        {
            CancelButtonClick(sender, e);
        }

        public void UpdateDefaults()
        {
            if (transaction.IsHighlighted)
            {
                ButtonsWithDelete.Visibility = Visibility.Visible;
                ButtonsWithoutDelete.Visibility = Visibility.Collapsed;

                UpdateButtons();
                UpdateCells();
            }

            if (transaction.IsReimbursement || transaction.StagedForReimbursement)
            {
                ReimbursementButton.Background = Brushes.DarkRed;
            }
        }

        private void UpdateButtons()
        {
            if (transaction.IsIncome)
            {
                IncomeButton.Background = (Brush)new BrushConverter().ConvertFrom("#33639E");
                selectedButton = IncomeButton;
            }
            else if (transaction.ISpending)
            {
                SpendingButton.Background = (Brush)new BrushConverter().ConvertFrom("#33639E");
                selectedButton = SpendingButton;
            }
            else if (transaction.IsExpense)
            {
                ExpensesButton.Background = (Brush)new BrushConverter().ConvertFrom("#33639E");
                selectedButton = ExpensesButton;
            }
        }

        private void UpdateCells()
        {
            int index = 0;
            foreach (ListItem item in list)
            {
                if (item.Name.All(char.IsDigit))
                {
                    if (transaction.Payee == item.Name)
                    {
                        index = 2;
                    }
                    else if (transaction.Particulars == item.Name)
                    {
                        index = 3;
                    }
                    else if (transaction.Code == item.Name)
                    {
                        index = 4;
                    }
                    else if (transaction.Reference == item.Name)
                    {
                        index = 5;
                    }
                }
                else
                {
                    if (transaction.Payee.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        index = 2;
                    }
                    else if (transaction.Particulars.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        index = 3;
                    }
                    else if (transaction.Code.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        index = 4;
                    }
                    else if (transaction.Reference.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        index = 5;
                    }
                }

                if (index != 0)
                {
                    this.item = item;
                    UpdateDefaultCellStyle(index);
                    break;
                }
            }
        }


        private void UpdateDefaultCellStyle(int index)
        {
            TransactionGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                var cell = TransactionGrid.Columns[index].GetCellContent(TransactionGrid.Items[0])?.Parent as DataGridCell;
                selectedCell = cell;
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
                unselectedCell.FontWeight = FontWeights.Medium;
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
            if (transaction.IsReimbursement)
            {
                OpenRemoveReimbursementWindow?.Invoke(transaction);
            }
            else if (transaction.StagedForReimbursement)
            {
                ReimbursementButton.Background = (Brush)new BrushConverter().ConvertFrom("#33639E");
                transaction.StagedForReimbursement = false;
            }
            else if (!transaction.StagedForReimbursement)
            {
                if (transaction2 is null)
                {
                    transaction.StagedForReimbursement = true;
                    ReturnTransaction?.Invoke(transaction);
                    GoBack?.Invoke(false);
                }
                else
                {
                    bool isValidReimbursement = (transaction1.Amount > 0 && transaction2.Amount < 0) ||
                        (transaction1.Amount < 0 && transaction2.Amount > 0);

                    if (!isValidReimbursement)
                    {
                        MessageBox.Show("One transaction needs to be positive, and the other needs to be negative.");
                        return;
                    }

                    OpenAddReimbursementWindow?.Invoke(transaction1, transaction2);
                }
            }
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            list.Remove(item);
            FileManagement.WriteList(list);
            GoBack?.Invoke(true);
        }

        private void ConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            if (selectedCell != null || selectedButton != null)
            {
                List<ListItem> list = FileManagement.ReadList();
                list.Add(new ListItem(Type, Category, Name));
                FileManagement.WriteList(list);
            }
            GoBack?.Invoke(true);
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            GoBack?.Invoke(false);
        }
    }
}
