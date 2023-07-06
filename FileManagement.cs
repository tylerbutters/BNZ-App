using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BNZApp
{
    public class FileManagement
    {
        public const string AccountFile = @"C:\Users\User\source\repos\BNZApp\Transactions.csv";
        public static List<Transaction> LoadTransactions()
        {
            if (!File.Exists(AccountFile)) // Handle the scenario where the account file is missing
            {
                throw new FileNotFoundException("file not found.", AccountFile);
            }

            List<string> rows = File.ReadAllLines(AccountFile).ToList();
            List<Transaction> transactions = new List<Transaction>();

            if (!(rows.Count >= 2)) //incase file is empty
            {
                throw new FormatException("file empty");
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
    }
}
