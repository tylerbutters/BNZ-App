using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNZApp
{
    public class Transaction : INotifyPropertyChanged
    {
        public string id { get; set; }
        public DateTime date { get; set; }
        public float amount { get; set; }
        public string payee { get; set; }
        public string particulars { get; set; }
        public string code { get; set; }
        public string reference { get; set; }
        public string transType { get; set; }
        public string formattedAmount { get => amount.ToString("C"); set { amount = float.Parse(value); OnPropertyChanged(nameof(formattedAmount)); } }
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
        public bool isNegative
        {
            get { return amount < 0; }
        }

        private bool isItemClicked;
        public bool IsItemClicked
        {
            get { return isItemClicked; }
            set
            {
                if (isItemClicked != value)
                {
                    isItemClicked = value;
                    OnPropertyChanged(nameof(IsItemClicked));
                }
            }
        }
        private bool isReimbursement;

        public bool IsReimbursement
        {
            get { return isReimbursement; }
            set
            {
                if (isReimbursement != value)
                {
                    isReimbursement = value;
                    OnPropertyChanged(nameof(IsReimbursement));
                }
            }
        }
        public override string ToString()
        {
            return $"{id},{date:dd/MM/yyyy},{amount},{payee},{particulars},{code},{reference},{transType}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
