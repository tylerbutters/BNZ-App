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
        public event EventHandler<RoutedEventArgs> GoBack;
        private bool isAddButtonClicked;
        private bool isDeleteButtonClicked;
        private List<string> list;
        private TransItemType type;
        public ViewListWindow(List<string> list, TransItemType type)
        {
            InitializeComponent();

            if(list is null)
            {
                throw new NullReferenceException();
            }          

            if (list.Count is 0 )
            {
                Console.Write("list of spending is empty");
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
            if (isAddButtonClicked || isDeleteButtonClicked)
            {
                FileManagement.WriteList(list, type);
            }
            GoBack?.Invoke(sender,e);
        }
        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            isAddButtonClicked = true;
            list.Add(NewItemInput.Text); 
            ListGrid.ItemsSource = null;
            ListGrid.ItemsSource = list;
        }
        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            isDeleteButtonClicked = true;
            string item = (sender as Button).DataContext as string;

            list.Remove(item);
            ListGrid.ItemsSource = null;
            ListGrid.ItemsSource = list;
        }
    }
}
