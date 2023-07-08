﻿using System;
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
        private const string ReimbursementsFile = @"CSVs\reimbursements.csv";
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

                if (split.Length != 8)
                {
                    throw new FormatException("row size greater or less than 7");
                }

                if (DateTime.Parse(split[1]).Year < 2023)
                {
                    continue;
                }

                Transaction transaction = new Transaction
                {
                    id = split[0],
                    date = DateTime.Parse(split[1]),
                    amount = float.Parse(split[2]),
                    payee = split[3],
                    particulars = split[4],
                    code = split[5],
                    reference = split[6],
                    transType = split[7],
                };

                if (transaction.amount is 0)
                {
                    throw new Exception("Amount = 0, this should never happen\nDate: " + transaction.date + "\nPayee: " + transaction.payee);
                }

                transactions.Add(transaction);
            }
            CheckIsReimbursement(transactions);

            return transactions;
        }
        private static void CheckIsReimbursement(List<Transaction> transactions)
        {
            List<Reimbursement> reimbursements = ReadReimbursements();
            foreach (Transaction transaction in transactions)
            {
                transaction.IsReimbursement = reimbursements.Any(reimbursement => reimbursement.transaction1 == transaction || reimbursement.transaction2 == transaction);
            }
        }


        public static List<Reimbursement> ReadReimbursements()
        {
            if (!File.Exists(ReimbursementsFile))
            {
                throw new FileNotFoundException("File not found.", ReimbursementsFile);
            }

            List<string> rows = File.ReadAllLines(ReimbursementsFile).ToList();
            List<Reimbursement> reimbursements = new List<Reimbursement>();

            if (rows.Count <= 1)
            {
                return reimbursements;
            }

            foreach (string row in rows.Skip(1))
            {
                string[] split = row.Split(',');

                if (split.Length != 16)
                {
                    throw new FormatException("Row size greater or less than 14");
                }

                Reimbursement reimbursement = new Reimbursement
                (
                    new Transaction //transaction 1
                    {
                        id = split[0],
                        date = DateTime.Parse(split[1]),
                        amount = float.Parse(split[2]),
                        payee = split[3],
                        particulars = split[4],
                        code = split[5],
                        reference = split[6],
                        transType = split[7]
                    },
                    new Transaction //transaction 2
                    {
                        id = split[8],
                        date = DateTime.Parse(split[9]),
                        amount = float.Parse(split[10]),
                        payee = split[11],
                        particulars = split[12],
                        code = split[13],
                        reference = split[14],
                        transType = split[15]
                    }
                );

                reimbursements.Add(reimbursement);
            }

            return reimbursements;
        }

        public static List<string> ReadList(TransItemType type)
        {
            string filePath = null;

            switch (type)
            {
                case TransItemType.Income:
                    filePath = ListOfIncomeFile;
                    break;
                case TransItemType.Spending:
                    filePath = ListOfSpendingFile;
                    break;
                case TransItemType.Expenses:
                    filePath = ListOfExpensesFile;
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
                    filePath = ListOfIncomeFile;
                    break;
                case TransItemType.Spending:
                    filePath = ListOfSpendingFile;
                    break;
                case TransItemType.Expenses:
                    filePath = ListOfExpensesFile;
                    break;
            }

            if (!File.Exists(filePath)) // Handle the scenario where the account file is missing
            {
                throw new FileNotFoundException("file not found.", filePath);
            }

            File.WriteAllLines(filePath, newList);
        }

        public static void WriteNewReimbursement(Reimbursement newReimbursement)
        {
            if (newReimbursement is null)
            {
                throw new NullReferenceException();
            }

            if (!File.Exists(ReimbursementsFile)) // Handle the scenario where the account file is missing
            {
                throw new FileNotFoundException("file not found.", ReimbursementsFile);
            }

            List<string> currentRows = File.ReadAllLines(ReimbursementsFile).ToList();
            currentRows.Add(newReimbursement.ToString());
            File.WriteAllLines(ReimbursementsFile, currentRows);
        }

        public static void WriteTransactions(List<Transaction> transactions)
        {
            if (transactions is null)
            {
                throw new NullReferenceException();
            }

            if (!File.Exists(TransactionsFile)) // Handle the scenario where the account file is missing
            {
                throw new FileNotFoundException("file not found.", TransactionsFile);
            }

            List<string> rows = new List<string>
            {
                "ID,Date,Amount,Payee,Particulars,Code,Reference,Tran Type"
            };
            foreach (Transaction transaction in transactions)
            {
                rows.Add(transaction.ToString());
            }

            File.WriteAllLines(TransactionsFile, rows);
        }
    }
}
