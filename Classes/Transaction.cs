using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

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
        public string formattedDate { get { return date.ToString("dd/MM/yy"); } }
        public string formattedAmount { get => amount.ToString("C"); set { amount = float.Parse(value); } }
        public string formattedTransType
        {
            get
            {
                switch (transType)
                {
                    case "AP":
                        return "Auto Payment"; //self created automatic payment
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
        public bool isNegative
        {
            get { return amount < 0; }
        }
        public bool isReimbursement { get; set; }
        public bool isItemClicked { get; set; }
        public bool isExpense { get; set; }
        public bool isIncome { get; set; }
        public bool isSpending { get; set; }
        public bool isHighlighted => isExpense || isIncome || isSpending;
        public override string ToString()
        {
            return $"{date:dd/MM/yyyy},{amount},{payee},{particulars},{code},{reference},{transType}";
        }

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is Transaction other))
            {
                return false;
            }

            return date == other.date
                && amount == other.amount
                && payee == other.payee
                && particulars == other.particulars
                && code == other.code
                && reference == other.reference
                && transType == other.transType;
        }
    }
}
