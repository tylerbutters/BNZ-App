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

        public Reimbursement(Transaction transaction1, Transaction transaction2)
        {
            this.transaction1 = transaction1;
            this.transaction2 = transaction2;
        }
    }
}
