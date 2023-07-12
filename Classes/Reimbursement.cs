using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNZApp
{
    public class Reimbursement
    {
        public Transaction transaction1 { get; set; }
        public Transaction transaction2 { get; set; }
        public override string ToString()
        {
            return $"{transaction1},{transaction2}";
        }
        public float ExcludeFromTotal(List<Transaction> transactions)
        {
            if (transactions.Any(transaction => transaction.id == transaction1.id && transaction.amount < 0))
            {
                return transaction1.amount;
            }
            else if (transactions.Any(transaction => transaction.id == transaction2.id && transaction.amount < 0))
            {
                return transaction2.amount;
            }
            return 0;
        }
        public Reimbursement(Transaction transaction1, Transaction transaction2)
        {
            if (transaction1.amount > 0) //if transaction 1 is positive
            {
                this.transaction1 = transaction1;
                this.transaction2 = transaction2;
            }
            else //if transaction 2 is positive
            {
                this.transaction1 = transaction2;
                this.transaction2 = transaction1;
            }
        }

    }
}
