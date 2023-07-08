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

                if (transaction.amount is 0)
                {
                    throw new Exception("Amount = 0, this should never happen\nDate: " + transaction.date + "\nPayee: " + transaction.payee);
                }

                transactions.Add(transaction);
            }
            return transactions;
        }

        public static List<string> ReadList(TransItemType type)
        {
            string filePath = null;

            switch (type)
            {
                case TransItemType.Income:
                    filePath = nameof(ListOfIncomeFile);
                    break;
                case TransItemType.Spending:
                    filePath = nameof(ListOfSpendingFile);
                    break;
                case TransItemType.Expenses:
                    filePath = nameof(ListOfExpensesFile);
                    break;
            }

            if (!File.Exists(filePath)) // Handle the scenario where the account file is missing
            {
                throw new FileNotFoundException("file not found.", filePath);
            }

            List<string> rows = File.ReadAllLines(filePath).ToList();
            List<string> list = new List<string>();

            if (rows.Count is 0) //incase file is empty
            {
                Console.WriteLine("file empty");
                return null;
            }

            foreach (string row in rows)
            {
                string[] split = row.Split(',');

                if (split.Length < 1)
                {
                    throw new FormatException("row size greater than 1");
                }

                list.Add(split[0]);
            }

            return list;
        }

        public static void WriteList(List<string> newList, TransItemType type)
        {
            if (newList is null)
            {
                throw new NullReferenceException();
            }

            string filePath = null;

            switch (type)
            {
                case TransItemType.Income:
                    filePath = nameof(ListOfIncomeFile);
                    break;
                case TransItemType.Spending:
                    filePath = nameof(ListOfSpendingFile);
                    break;
                case TransItemType.Expenses:
                    filePath = nameof(ListOfExpensesFile);
                    break;
            }

            if (!File.Exists(filePath)) // Handle the scenario where the account file is missing
            {
                throw new FileNotFoundException("file not found.", filePath);
            }

            File.WriteAllLines(filePath, newList);
        }
    }
}
