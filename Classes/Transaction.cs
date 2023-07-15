using System;
using System.ComponentModel;

namespace BNZApp
{
    public class Transaction : INotifyPropertyChanged
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Payee { get; set; }
        public string Particulars { get; set; }
        public string Code { get; set; }
        public string Reference { get; set; }
        public string TransactionType { get; set; }

        public string FormattedDate => Date.ToString("dd/MM/yy");
        public string FormattedAmount { get => Amount.ToString("C"); set => Amount = decimal.Parse(value); }
        public string FormattedTransType
        {
            get
            {
                switch (TransactionType)
                {
                    case "AP":
                        return "Auto Payment";
                    case "DC":
                        return "Debit Card";
                    case "FT":
                        return "Funds Transfer";
                    case "POS":
                        return "Point of Sale";
                    case "DD":
                        return "Direct Debit";
                    case "BP":
                        return "Bill Payment";
                    case "ATM":
                        return "ATM";
                    default:
                        throw new Exception("Code is null or does not fit any options");
                }
            }
        }

        public bool IsNegative => Amount < 0;
        public bool IsReimbursement { get; set; }
        public bool IsExpense { get; set; }
        public bool IsIncome { get; set; }
        public bool ISpending { get; set; }
        public bool IsHighlighted => IsExpense || IsIncome || ISpending;

        public bool StagedForReimbursement { get => stagedForReimbursement; set { stagedForReimbursement = value; OnPropertyChanged(nameof(StagedForReimbursement)); } }
        private bool stagedForReimbursement;

        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is Transaction other))
            {
                return false;
            }

            return Date == other.Date
                && Amount == other.Amount
                && Payee == other.Payee
                && Particulars == other.Particulars
                && Code == other.Code
                && Reference == other.Reference
                && TransactionType == other.TransactionType;
        }

        public override string ToString() => $"{Date:dd/MM/yyyy},{Amount},{Payee},{Particulars},{Code},{Reference},{TransactionType}";

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
