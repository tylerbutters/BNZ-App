namespace BNZApp {
    public class DetailsItem
    {
        public string name { get; set; }
        public string formattedName { get { return name.ToUpper() + ":"; } }
        public float amount { get; set; }
        public string formattedAmount { get { return amount.ToString("C"); } }
        public bool isZero { get { return amount is 0; } }
        public DetailsItem(string name, float amount)
        {
            this.name = name;
            this.amount = amount;
        }
    }
}
