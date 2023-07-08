using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace BNZApp
{
    public class FileManagement
    {
        private const string TransactionsFile = @"CSVs\transactions.csv";
        private const string ListOfExpensesFile = @"CSVs\list-of-expenses.csv";
        private const string ListOfIncomeFile = @"CSVs\list-of-income.csv";
        private const string ListOfSpendingFile = @"CSVs\list-of-spending.csv";

        public static List<Transaction> ReadTransactions()
        {
            if (!File.Exists(TransactionsFile)) // Handle the scenario where the account file is missing
            {
                throw new FileNotFoundException("file not found.", TransactionsFile);
            }

            List<string> rows = File.ReadAllLines(TransactionsFile).ToList();
            List<Transaction> transactions = new List<Transaction>();

            if (rows.Count is 1) //incase file is empty
            {
                MessageBox.Show("No transactions in file");
                return null;
            }

            foreach (string row in rows.Skip(1))
            {
                string[] split = row.Split(',');

                if (split.Length != 7)
                {
                    throw new FormatException("row size greater or less than 7");
                }

                if (DateTime.Parse(split[0]).Year < 2023)
                {
                    continue;
                }

                Transaction transaction = new Transaction
                {
                    date = DateTime.Parse(split[0]),
                    amount = float.Parse(split[1]),
                    payee = split[2],
                    particulars = split[3],
                    code = split[4],
                    reference = split[5],
                    transType = split[6],
                };

                if (transaction.amount == 0)
                {
                    throw new Exception("Amount = 0, this should never happen\nDate: " + transaction.date + "\nPayee: " + transaction.payee);
                }

                transactions.Add(transaction);
            }
            return transactions;
        }

        public static List<string> ReadListOfIncome()
        {
            if (!File.Exists(ListOfIncomeFile)) // Handle the scenario where the account file is missing
            {
                throw new FileNotFoundException("file not found.", ListOfIncomeFile);
            }

            List<string> rows = File.ReadAllLines(ListOfIncomeFile).ToList();
            List<string> listOfIncome = new List<string>();

            if (rows.Count is 0) //incase file is empty
            {
                Console.WriteLine("Income file empty");
                return null;
            }

            foreach (string row in rows)
            {
                string[] split = row.Split(',');

                if (split.Length < 1)
                {
                    throw new FormatException("row size greater than 1");
                }

                listOfIncome.Add(split[0]);
            }

            return listOfIncome;
        }

        public static List<string> ReadListOfSpending()
        {
            if (!File.Exists(ListOfSpendingFile)) // Handle the scenario where the account file is missing
            {
                throw new FileNotFoundException("file not found.", ListOfSpendingFile);
            }

            List<string> rows = File.ReadAllLines(ListOfSpendingFile).ToList();
            List<string> listOfSpending = new List<string>();

            if (rows.Count is 0) //incase file is empty
            {
                Console.WriteLine("Spending file empty");
                return null;
            }

            foreach (string row in rows)
            {
                string[] split = row.Split(',');

                if (split.Length < 1)
                {
                    throw new FormatException("row size greater than 1");
                }

                listOfSpending.Add(split[0]);
            }

            return listOfSpending;
        }

        public static List<string> ReadListOfExpenses()
        {
            if (!File.Exists(ListOfExpensesFile)) // Handle the scenario where the account file is missing
            {
                throw new FileNotFoundException("file not found.", ListOfExpensesFile);
            }

            List<string> rows = File.ReadAllLines(ListOfExpensesFile).ToList();
            List<string> listOfExpenses = new List<string>();

            if (rows.Count is 0) //incase file is empty
            {
                Console.WriteLine("Expenses file empty");
                return null;
            }

            foreach (string row in rows)
            {
                string[] split = row.Split(',');

                if (split.Length < 1)
                {
                    throw new FormatException("row size greater than 1");
                }

                listOfExpenses.Add(split[0]);
            }

            return listOfExpenses;
        }

        public static void WriteListOfSpending(List<string> newListOfSpending)
        {
            if (newListOfSpending is null)
            {
                throw new NullReferenceException();
            }

            File.WriteAllLines(ListOfSpendingFile, newListOfSpending);
        }
        public static void WriteListOfIncome(List<string> newListOfIncome)
        {
            if (newListOfIncome is null)
            {
                throw new NullReferenceException();
            }

            File.WriteAllLines(ListOfIncomeFile, newListOfIncome);
        }
        public static void WriteListOfExpenses(List<string> newListOfExpenses)
        {
            if (newListOfExpenses is null)
            {
                throw new NullReferenceException();
            }

            File.WriteAllLines(ListOfExpensesFile, newListOfExpenses);
        }

    }
}
