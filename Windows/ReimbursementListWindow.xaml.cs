using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for ReimbursementListWindow.xaml
    /// </summary>
    public partial class ReimbursementListWindow : Page
    {
        private bool isDeleteButtonClicked;
        private List<Reimbursement> reimbursements = FileManagement.ReadReimbursements();
        public event Action<bool> GoBack;

        public ReimbursementListWindow()
        {
            InitializeComponent();

            if (reimbursements.Count is 0)
            {
                Console.WriteLine("The list of reimbursements is empty.");
            }

            ReimbursementGrid.ItemsSource = reimbursements;
        }

        private void BackgroundClick(object sender, MouseButtonEventArgs e)
        {
            DoneButtonClick(sender, e);
        }

        private void DoneButtonClick(object sender, RoutedEventArgs e)
        {
            if (isDeleteButtonClicked)
            {
                FileManagement.WriteReimbursements(reimbursements);
            }

            GoBack?.Invoke(isDeleteButtonClicked);
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            Reimbursement selectedReimbursement = (sender as Button)?.DataContext as Reimbursement;
            if (selectedReimbursement is null)
            {
                throw new ArgumentNullException(nameof(selectedReimbursement), "The selected reimbursement is null.");
            }

            reimbursements.Remove(selectedReimbursement);
            ReimbursementGrid.ItemsSource = null;
            ReimbursementGrid.ItemsSource = reimbursements;
            isDeleteButtonClicked = true;
        }

    }
}
