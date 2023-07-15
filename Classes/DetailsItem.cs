namespace BNZApp
{
    public class DetailsItem
    {
        public string Name { get; set; }
        public string FormattedName => $"{Name.ToUpper()}:";
        public decimal Amount { get; set; }
        public string FormattedAmount => Amount.ToString("C");
        public bool IsZero => Amount == 0;

        public DetailsItem(string name, decimal amount)
        {
            Name = name;
            Amount = amount;
        }
    }
}