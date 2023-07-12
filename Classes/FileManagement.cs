using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BNZApp
{
    public class FileManagement
    {
        public const string TransactionsFile = @"CSVs\transactions.csv";
        private const string ReimbursementsFile = @"CSVs\reimbursements.csv";
        private const string ListOfItemsFile = @"CSVs\list-of-items.csv";

        public static List<Transaction> ReadNewFile()
        {
            if (!File.Exists(TransactionsFile))
            {
                throw new FileNotFoundException("Transactions file not found.", TransactionsFile);
            }

            List<string> lines = File.ReadAllLines(TransactionsFile).ToList();
            List<Transaction> transactions = new List<Transaction>();

            if (lines.Count == 0)
            {
                throw new FormatException("File is empty");
            }

            foreach (string row in lines.Skip(1))
            {
                string[] split = row.Split(',');

                if (split.Length != 7)
                {
                    throw new FormatException("File format is invalid");
                }

                if (DateTime.TryParse(split[1], out DateTime date) && date.Year < 2023)
                {
                    continue;
                }

                Transaction transaction = new Transaction
                {
                    date = date,
                    amount = float.Parse(split[1]),
                    payee = split[2],
                    particulars = split[3],
                    code = split[4],
                    reference = split[5],
                    transType = split[6],
                };

                if (transaction.amount == 0)
                {
                    throw new Exception($"Amount = 0, this should never happen\nDate: {transaction.date}\nPayee: {transaction.payee}");
                }

                transactions.Add(transaction);
            }

            return transactions;
        }

        public static List<Transaction> ReadTransactions()
        {
            if (!File.Exists(TransactionsFile))
            {
                throw new FileNotFoundException("Transactions file not found.", TransactionsFile);
            }

            List<string> lines = File.ReadAllLines(TransactionsFile).ToList();
            List<Transaction> transactions = new List<Transaction>();

            if (lines.Count is 0)
            {
                throw new FormatException("File is empty");
            }

            if (lines.Count is 1)
            {
                return transactions;
            }

            foreach (string row in lines.Skip(1))
            {
                string[] split = row.Split(',');

                if (split.Length != 8)
                {
                    throw new FormatException("Row size is greater or less than 8");
                }

                if (DateTime.TryParse(split[1], out DateTime date) && date.Year < 2023)
                {
                    continue;
                }

                Transaction transaction = new Transaction
                {
                    id = split[0],
                    date = date,
                    amount = float.Parse(split[2]),
                    payee = split[3],
                    particulars = split[4],
                    code = split[5],
                    reference = split[6],
                    transType = split[7],
                };

                if (transaction.amount == 0)
                {
                    throw new Exception($"Amount = 0, this should never happen\nDate: {transaction.date}\nPayee: {transaction.payee}");
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
                transaction.IsReimbursement = reimbursements.Any(reimbursement => reimbursement.transaction1.id == transaction.id || reimbursement.transaction2.id == transaction.id);
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

            if (lines.Count <= 1)
            {
                return reimbursements;
            }

            foreach (string row in lines.Skip(1))
            {
                string[] split = row.Split(',');

                if (split.Length != 16)
                {
                    throw new FormatException("Row size is greater or less than 16");
                }

                Reimbursement reimbursement = new Reimbursement(
                    new Transaction
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
                    new Transaction
                    {
                        id = split[8],
                        date = DateTime.Parse(split[9]),
                        amount = float.Parse(split[10]),
                        payee = split[11],
                        particulars = split[12],
                        code = split[13],
                        reference = split[14],
                        transType = split[15]
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

            if (lines.Count == 0)
            {
                Console.WriteLine("File is empty");
                return list;
            }

            foreach (string row in lines.Skip(1))
            {
                string[] split = row.Split(',');

                if (split.Length < 1)
                {
                    throw new FormatException("Row size is greater than 1");
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

            List<string> lines = new List<string> { "ID,Date,Amount,Payee,Particulars,Code,Reference,Transaction Type" };
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

            List<string> lines = new List<string> { "ID,Date,Amount,Payee,Particulars,Code,Reference,Tran Type,ID,Date,Amount,Payee,Particulars,Code,Reference,Tran Type" };
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
