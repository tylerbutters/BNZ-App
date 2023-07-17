using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace BNZApp
{
    public class FileManagement
    {
        private static readonly string FilesDirectory = "DataFiles";
        private static readonly string TransactionsFile = "transactions.csv";
        private static readonly string ReimbursementsFile = "reimbursements.csv";
        private static readonly string ItemsFile = "items.csv";
        private static readonly string ProfileFile = "profile.csv";
        private static readonly string TransactionsFilePath = Path.Combine(Environment.CurrentDirectory, FilesDirectory, TransactionsFile);
        private static readonly string ReimbursementsFilePath = Path.Combine(Environment.CurrentDirectory, FilesDirectory, ReimbursementsFile);
        private static readonly string ItemsFilePath = Path.Combine(Environment.CurrentDirectory, FilesDirectory, ItemsFile);
        private static readonly string ProfileFilePath = Path.Combine(Environment.CurrentDirectory, FilesDirectory, ProfileFile);


        public static List<Transaction> ReadNewFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found.", filePath);
            }

            string[] lines = File.ReadAllLines(filePath);
            List<Transaction> transactions = new List<Transaction>();

            foreach (string row in lines.Skip(1))
            {
                string[] split = row.Split(',');

                if (split.Length != 7)
                {
                    return null;
                }

                if (!DateTime.TryParse(split[0], out DateTime date))
                {
                    return null;
                }

                if (!decimal.TryParse(split[1], out decimal amount))
                {
                    return null;
                }

                Transaction transaction = new Transaction
                {
                    Date = date,
                    Amount = amount,
                    Payee = split[2],
                    Particulars = split[3],
                    Code = split[4],
                    Reference = split[5],
                    TransactionType = split[6],
                };

                if (transaction.Amount is 0)
                {
                    throw new Exception($"Amount = 0, this should never happen\nDate: {transaction.Date}\nPayee: {transaction.Payee}");
                }

                transactions.Add(transaction);
            }
            CheckIsReimbursement(transactions);

            return transactions;
        }

        public static List<Transaction> ReadTransactions()
        {
            if (!File.Exists(TransactionsFilePath))
            {
                throw new FileNotFoundException("Transactions file not found.", TransactionsFilePath);
            }

            string[] lines = File.ReadAllLines(TransactionsFilePath);
            List<Transaction> transactions = new List<Transaction>();

            foreach (string row in lines.Skip(1))
            {
                string[] split = row.Split(',');

                if (split.Length != 7)
                {
                    return null;
                }

                if (DateTime.Parse(split[0]).Year < 2023) //for testing
                {
                    continue;
                }

                if (!DateTime.TryParse(split[0], out DateTime date))
                {
                    return null;
                }

                if (!decimal.TryParse(split[1], out decimal amount))
                {
                    return null;
                }

                Transaction transaction = new Transaction
                {
                    Date = date,
                    Amount = amount,
                    Payee = split[2],
                    Particulars = split[3],
                    Code = split[4],
                    Reference = split[5],
                    TransactionType = split[6],
                };

                if (transaction.Amount is 0)
                {
                    throw new Exception($"Amount = 0, this should never happen\nDate: {transaction.Date}\nPayee: {transaction.Payee}");
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
                transaction.IsReimbursement = reimbursements.Any(reimbursement => transaction.Equals(reimbursement.Transaction1) || transaction.Equals(reimbursement.Transaction2));
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

                    if (item.Category.Equals("payee", StringComparison.OrdinalIgnoreCase))
                    {
                        isMatch = (transaction.Payee.All(char.IsDigit) && transaction.Payee == item.Name) ||
                                  (!transaction.Payee.All(char.IsDigit) && transaction.Payee.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                    else if (item.Category.Equals("particulars", StringComparison.OrdinalIgnoreCase))
                    {
                        isMatch = (transaction.Particulars.All(char.IsDigit) && transaction.Particulars == item.Name) ||
                                  (!transaction.Particulars.All(char.IsDigit) && transaction.Particulars.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                    else if (item.Category.Equals("code", StringComparison.OrdinalIgnoreCase))
                    {
                        isMatch = (transaction.Code.All(char.IsDigit) && transaction.Code == item.Name) ||
                                  (!transaction.Code.All(char.IsDigit) && transaction.Code.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                    else if (item.Category.Equals("reference", StringComparison.OrdinalIgnoreCase))
                    {
                        isMatch = (transaction.Reference.All(char.IsDigit) && transaction.Reference == item.Name) ||
                                  (!transaction.Reference.All(char.IsDigit) && transaction.Reference.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                    else
                    {
                        throw new ArgumentException("Item type is not valid", nameof(item.Category));
                    }

                    if (isMatch)
                    {
                        switch (item.ListType)
                        {
                            case ListType.Income:
                                transaction.IsIncome = true;
                                break;
                            case ListType.Spending:
                                transaction.ISpending = true;
                                break;
                            case ListType.Expenses:
                                transaction.IsExpense = true;
                                break;
                        }
                    }
                }
            }
        }


        public static List<Reimbursement> ReadReimbursements()
        {
            if (!File.Exists(ReimbursementsFilePath))
            {
                throw new FileNotFoundException("Reimbursements file not found.", ReimbursementsFilePath);
            }

            string[] lines = File.ReadAllLines(ReimbursementsFilePath);
            List<Reimbursement> reimbursements = new List<Reimbursement>();

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
                        Date = DateTime.TryParse(split[0], out DateTime date1) ? date1 : throw new FormatException("Invalid date format"),
                        Amount = decimal.TryParse(split[1], out decimal amount1) ? amount1 : throw new FormatException("Invalid decimal format"),
                        Payee = split[2],
                        Particulars = split[3],
                        Code = split[4],
                        Reference = split[5],
                        TransactionType = split[6]
                    },
                    new Transaction
                    {
                        Date = DateTime.TryParse(split[7], out DateTime date2) ? date2 : throw new FormatException("Invalid date format"),
                        Amount = decimal.TryParse(split[8], out decimal amount2) ? amount2 : throw new FormatException("Invalid decimal format"),
                        Payee = split[9],
                        Particulars = split[10],
                        Code = split[11],
                        Reference = split[12],
                        TransactionType = split[13]
                    });

                reimbursements.Add(reimbursement);
            }

            return reimbursements;
        }

        public static List<ListItem> ReadList()
        {
            if (!File.Exists(ItemsFilePath))
            {
                throw new FileNotFoundException("List file not found.", ItemsFilePath);
            }

            string[] lines = File.ReadAllLines(ItemsFilePath);
            List<ListItem> list = new List<ListItem>();

            foreach (string line in lines.Skip(1))
            {
                string[] split = line.Split(',');

                if (split.Length != 3)
                {
                    throw new FormatException("Row size is greater than 3");
                }

                list.Add(new ListItem((ListType)Enum.Parse(typeof(ListType), split[0]), split[1], split[2]));
            }

            return list;
        }

        public static List<string> ReadProfile()
        {
            if (!File.Exists(ProfileFilePath))
            {
                throw new FileNotFoundException("Profile file not found.", ProfileFilePath);
            }

            List<string> lines = File.ReadAllLines(ProfileFilePath).ToList();

            return lines;
        }

        public static void WriteProfile(List<string> profile)
        {
            if (!File.Exists(ProfileFilePath))
            {
                throw new FileNotFoundException("List file not found.", ProfileFilePath);
            }


            File.WriteAllLines(ProfileFilePath, profile);
        }

        public static void WriteList(List<ListItem> list)
        {
            if (list is null)
            {
                throw new NullReferenceException("New list is null");
            }

            if (!File.Exists(ItemsFilePath))
            {
                throw new FileNotFoundException("List file not found.", ItemsFilePath);
            }

            List<string> lines = new List<string> { "List Type,Category,Name" };
            foreach (ListItem item in list)
            {
                lines.Add(item.ToString());
            }

            File.WriteAllLines(ItemsFilePath, lines);
        }

        public static void WriteTransactions(List<Transaction> transactions)
        {
            if (transactions is null)
            {
                throw new NullReferenceException("Transactions list is null");
            }

            if (!File.Exists(TransactionsFilePath))
            {
                throw new FileNotFoundException("Transactions file not found.", TransactionsFilePath);
            }

            List<string> lines = new List<string> { "Date,Amount,Payee,Particulars,Code,Reference,Transaction Type" };
            foreach (Transaction transaction in transactions)
            {
                lines.Add(transaction.ToString());
            }

            File.WriteAllLines(TransactionsFilePath, lines);
        }

        public static void WriteReimbursements(List<Reimbursement> reimbursements)
        {
            if (reimbursements is null)
            {
                throw new NullReferenceException("Reimbursements list is null");
            }

            if (!File.Exists(ReimbursementsFilePath))
            {
                throw new FileNotFoundException("Reimbursements file not found.", ReimbursementsFilePath);
            }

            List<string> lines = new List<string> { "Date,Amount,Payee,Particulars,Code,Reference,Tran Type,Date,Amount,Payee,Particulars,Code,Reference,Tran Type" };
            foreach (Reimbursement reimbursement in reimbursements)
            {
                lines.Add(reimbursement.ToString());
            }

            File.WriteAllLines(ReimbursementsFilePath, lines);
        }

        public static void ClearData()
        {
            WriteList(new List<ListItem>());
            WriteTransactions(new List<Transaction>());
            WriteReimbursements(new List<Reimbursement>());
        }

    }
}
