using System;

namespace BNZApp
{
    public class Reimbursement
    {
        public Transaction transaction1 { get; set; } //positive
        public Transaction transaction2 { get; set; } //negative
        public override string ToString()
        {
            return $"{transaction1},{transaction2}";
        }
        public float ExcludeFromTotal(Transaction transaction)
        {
            if (transaction.amount < 0 && (transaction.Equals(transaction1) || transaction.Equals(transaction2)))
            {
                if(Math.Abs(transaction1.amount) > Math.Abs(transaction2.amount))
                {
                    return transaction2.amount;
                }
                else
                {
                    return -transaction1.amount;
                }
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
