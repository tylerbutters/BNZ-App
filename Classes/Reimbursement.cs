using System;

namespace BNZApp
{
    public class Reimbursement
    {
        public Transaction Transaction1 { get; set; } // Positive transaction
        public Transaction Transaction2 { get; set; } // Negative transaction

        public decimal ExcludeAmount(Transaction transaction)
        {
            if (transaction.Amount < 0 && (transaction.Equals(Transaction1) || transaction.Equals(Transaction2)))
            {
                if (Math.Abs(Transaction1.Amount) > Math.Abs(Transaction2.Amount))
                {
                    return Transaction2.Amount;
                }
                else
                {
                    return -Transaction1.Amount;
                }
            }
            return 0;
        }

        public override string ToString() => $"{Transaction1},{Transaction2}";

        public Reimbursement(Transaction transaction1, Transaction transaction2)
        {
            if (transaction1.Amount > 0) // If transaction 1 is positive
            {
                Transaction1 = transaction1;
                Transaction2 = transaction2;
            }
            else // If transaction 2 is positive
            {
                Transaction1 = transaction2;
                Transaction2 = transaction1;
            }
        }
    }
}