using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BNZApp
{
    /// <summary>
    /// Interaction logic for ReimbursementListWindow.xaml
    /// </summary>
    public partial class ReimbursementListWindow : Page
    {
        private bool isDeleteButtonClicked;
        private List<Reimbursement> reimbursements = FileManagement.ReadReimbursements();
        public event EventHandler<bool> GoBack;

        public ReimbursementListWindow()
        {
            InitializeComponent();

            if (reimbursements.Count is 0)
            {
                Console.WriteLine("The list of reimbursements is empty.");
            }

            ListGrid.ItemsSource = reimbursements;
        }

        private void DoneButtonClick(object sender, RoutedEventArgs e)
        {
            if (isDeleteButtonClicked)
            {
                FileManagement.WriteReimbursements(reimbursements);
            }

            GoBack?.Invoke(sender, isDeleteButtonClicked);
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            Reimbursement selectedReimbursement = (sender as Button)?.DataContext as Reimbursement;
            if (selectedReimbursement is null)
            {
                throw new ArgumentNullException(nameof(selectedReimbursement), "The selected reimbursement is null.");
            }

            reimbursements.Remove(selectedReimbursement);
            ListGrid.ItemsSource = null;
            ListGrid.ItemsSource = reimbursements;
            isDeleteButtonClicked = true;
        }

    }
}
