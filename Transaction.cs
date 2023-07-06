using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNZApp
{
    public class Transaction
    {
        public DateTime date { get; set; }
        public float amount { get; set; }
        public string payee { get; set; }
        public string particulars { get; set; }
        public string code { get; set; }
        public string reference { get; set; }
        public string transType { get; set; }
        public string formattedAmount
        {
            get
            {
                string formattedAmount = Math.Abs(amount).ToString("C");

                if (amount < 0)
                {
                    return "-" + formattedAmount.TrimStart('-');
                }
                else
                {
                    return formattedAmount;
                }
            }
        }
        public string formattedTransType
        {
            get
            {
                switch (transType)
                {
                    case "AP":
                        return "Automatic Payment"; //self created automatic payment
                    case "DC":
                        return "Debit Card"; //ingrid
                    case "FT":
                        return "Funds Transfer"; //transfer between accounts
                    case "POS":
                        return "Point of Sale"; //newworld, skinny
                    case "DD":
                        return "Direct Debit"; //city fitness
                    case "BP":
                        return "Bill Payment"; //send to payee or work
                    case "ATM":
                        return "ATM";
                    default:
                        throw new Exception("code is null or does not fit any options");
                }
            }
        }
    }
}
