﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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