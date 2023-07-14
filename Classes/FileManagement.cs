using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace BNZApp
{
    public class FileManagement
    {
        public const string TransactionsFile = @"CSVs\transactions.csv";
        private const string ReimbursementsFile = @"CSVs\reimbursements.csv";
        private const string ListOfItemsFile = @"CSVs\list-of-items.csv";

        public static List<Transaction> ReadNewFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }

            List<string> lines = File.ReadAllLines(filePath).ToList();

            if (lines is null || lines.Count is 0)
            {
                MessageBox.Show("Invalid file\nFile is empty");
                return null;
            }

            List<Transaction> transactions = new List<Transaction>();
            foreach (string row in lines.Skip(1))
            {
                string[] split = row.Split(',');

                if (split.Length != 7)
                {
                    MessageBox.Show("Row size is greater or less than 8");
                    return null;
                }

                if (!DateTime.TryParse(split[0], out DateTime date))
                {
                    MessageBox.Show($"Invalid date format: {split[0]}");
                    return null;
                }

                if (!float.TryParse(split[1], out float amount))
                {
                    MessageBox.Show($"Invalid float format: {split[1]}");
                    return null;
                }

                Transaction transaction = new Transaction
                {
                    date = date,
                    amount = amount,
                    payee = split[2],
                    particulars = split[3],
                    code = split[4],
                    reference = split[5],
                    transType = split[6],
                };

                if (transaction.amount is 0)
                {
                    throw new Exception($"Amount = 0, this should never happen\nDate: {transaction.date}\nPayee: {transaction.payee}");
                }

                transactions.Add(transaction);
            }
            CheckIsReimbursement(transactions);

            return transactions;
        }
        public static List<Transaction> ReadTransactions()
        {
            if (!File.Exists(TransactionsFile))
            {
                throw new FileNotFoundException("Transactions file not found.", TransactionsFile);
            }

            List<string> lines = File.ReadAllLines(TransactionsFile).ToList();

            if (lines is null || lines.Count is 0)
            {
                MessageBox.Show("Invalid file\nFile is empty");
                return null;
            }

            List<Transaction> transactions = new List<Transaction>();

            foreach (string row in lines.Skip(1))
            {
                string[] split = row.Split(',');

                if (split.Length != 7)
                {
                    MessageBox.Show("Row size is greater or less than 8");
                    return null;
                }

                if (DateTime.Parse(split[0]).Year < 2023) //for testing
                {
                    continue;
                }

                if (!DateTime.TryParse(split[0], out DateTime date))
                {
                    MessageBox.Show($"Invalid date format: {split[0]}");
                    return null;
                }

                if (!float.TryParse(split[1], out float amount))
                {
                    MessageBox.Show($"Invalid float format: {split[1]}");
                    return null;
                }

                Transaction transaction = new Transaction
                {
                    date = date,
                    amount = amount,
                    payee = split[2],
                    particulars = split[3],
                    code = split[4],
                    reference = split[5],
                    transType = split[6],
                };

                if (transaction.amount is 0)
                {
                    throw new Exception($"Amount = 0, this should never happen\nDate: {transaction.date}\nPayee: {transaction.payee}");
                }

                transactions.Add(transaction);
            }
            CheckIsReimbursement(transactions);
            CheckIsOnList(transactions);

            return transactions;
        }

        private static void CheckIsReimbursement(List<Transaction> transactions)
        {
            List<Reimbursement> reimbursements = ReadReimbursements();
            foreach (Transaction transaction in transactions)
            {
                transaction.isReimbursement = reimbursements.Any(reimbursement => transaction.Equals(reimbursement.transaction1) || transaction.Equals(reimbursement.transaction2));
            }
        }
        private static void CheckIsOnList(List<Transaction> transactions)
        {
            List<ListItem> listItems = ReadList();

            foreach (Transaction transaction in transactions)
            {
                foreach (ListItem item in listItems)
                {
                    bool isMatch = false;

                    switch (item.category)
                    {
                        case "payee":
                            isMatch = transaction.payee.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0;
                            break;
                        case "particulars":
                            isMatch = transaction.particulars.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0;
                            break;
                        case "code":
                            isMatch = transaction.code.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0;
                            break;
                        case "reference":
                            isMatch = transaction.reference.IndexOf(item.name, StringComparison.OrdinalIgnoreCase) >= 0;
                            break;
                        default:
                            throw new ArgumentException("Item type is not valid", nameof(item.category));
                    }

                    if (isMatch)
                    {
                        switch (item.listType)
                        {
                            case ListType.Income:
                                transaction.isIncome = true;
                                break;
                            case ListType.Spending:
                                transaction.isSpending = true;
                                break;
                            case ListType.Expenses:
                                transaction.isExpense = true;
                                break;
                        }
                    }
                }
            }
        }

        public static List<Reimbursement> ReadReimbursements()
        {
            if (!File.Exists(ReimbursementsFile))
            {
                throw new FileNotFoundException("Reimbursements file not found.", ReimbursementsFile);
            }

            List<string> lines = File.ReadAllLines(ReimbursementsFile).ToList();
            List<Reimbursement> reimbursements = new List<Reimbursement>();

            if (lines is null || lines.Count is 0)
            {
                throw new FormatException("File is empty");
            }

            if (lines.Count is 1)
            {
                return reimbursements;
            }

            foreach (string row in lines.Skip(1))
            {
                string[] split = row.Split(',');

                if (split.Length != 14)
                {
                    throw new FormatException("Row size is greater or less than 16");
                }

                Reimbursement reimbursement = new Reimbursement(
                    new Transaction
                    {
                        date = DateTime.TryParse(split[0], out DateTime date1) ? date1 : throw new FormatException("Invalid date format"),
                        amount = float.TryParse(split[1], out float amount1) ? amount1 : throw new FormatException("Invalid float format"),
                        payee = split[2],
                        particulars = split[3],
                        code = split[4],
                        reference = split[5],
                        transType = split[6]
                    },
                    new Transaction
                    {
                        date = DateTime.TryParse(split[7], out DateTime date2) ? date2 : throw new FormatException("Invalid date format"),
                        amount = float.TryParse(split[8], out float amount2) ? amount2 : throw new FormatException("Invalid float format"),
                        payee = split[9],
                        particulars = split[10],
                        code = split[11],
                        reference = split[12],
                        transType = split[13]
                    });

                reimbursements.Add(reimbursement);
            }

            return reimbursements;
        }

        public static List<ListItem> ReadList()
        {
            if (!File.Exists(ListOfItemsFile))
            {
                throw new FileNotFoundException("List file not found.", ListOfItemsFile);
            }

            List<string> lines = File.ReadAllLines(ListOfItemsFile).ToList();
            List<ListItem> list = new List<ListItem>();

            if (lines is null || lines.Count is 0)
            {
                throw new FormatException("File is empty");
            }

            if (lines.Count is 1)
            {
                Console.WriteLine("File is empty");
                return list;
            }

            foreach (string row in lines.Skip(1))
            {
                string[] split = row.Split(',');

                if (split.Length != 3)
                {
                    throw new FormatException("Row size is greater than 3");
                }

                list.Add(new ListItem((ListType)Enum.Parse(typeof(ListType), split[0]), split[1], split[2]));
            }

            return list;
        }

        public static void WriteList(List<ListItem> list)
        {
            if (list is null)
            {
                throw new NullReferenceException("New list is null");
            }

            if (!File.Exists(ListOfItemsFile))
            {
                throw new FileNotFoundException("List file not found.", ListOfItemsFile);
            }

            List<string> lines = new List<string> { "List Type,Category,Name" };
            foreach (ListItem item in list)
            {
                lines.Add(item.ToString());
            }

            File.WriteAllLines(ListOfItemsFile, lines);
        }

        public static void WriteTransactions(List<Transaction> transactions)
        {
            if (transactions is null)
            {
                throw new NullReferenceException("Transactions list is null");
            }

            if (!File.Exists(TransactionsFile))
            {
                throw new FileNotFoundException("Transactions file not found.", TransactionsFile);
            }

            List<string> lines = new List<string> { "Date,Amount,Payee,Particulars,Code,Reference,Transaction Type" };
            foreach (Transaction transaction in transactions)
            {
                lines.Add(transaction.ToString());
            }

            File.WriteAllLines(TransactionsFile, lines);
        }

        public static void WriteReimbursements(List<Reimbursement> reimbursements)
        {
            if (reimbursements is null)
            {
                throw new NullReferenceException("Reimbursements list is null");
            }

            if (!File.Exists(ReimbursementsFile))
            {
                throw new FileNotFoundException("Reimbursements file not found.", ReimbursementsFile);
            }

            List<string> lines = new List<string> { "Date,Amount,Payee,Particulars,Code,Reference,Tran Type,Date,Amount,Payee,Particulars,Code,Reference,Tran Type" };
            foreach (Reimbursement reimbursement in reimbursements)
            {
                lines.Add(reimbursement.ToString());
            }

            File.WriteAllLines(ReimbursementsFile, lines);
        }

        public static void ClearData()
        {
            WriteList(new List<ListItem>());
            WriteTransactions(new List<Transaction>());
            WriteReimbursements(new List<Reimbursement>());
        }

    }
}
