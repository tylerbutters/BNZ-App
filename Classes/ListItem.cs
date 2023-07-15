namespace BNZApp
{
    public class ListItem
    {
        public ListType ListType { get; set; }
        public string Category { get => category; set => category = value.ToLower(); }
        private string category;
        public string Name { get => name; set => name = value.ToLower(); }
        private string name;
        public string FormattedName => Name.ToUpper();
        public string FormattedCategory => char.ToUpper(Category[0]) + Category.Substring(1);

        public override string ToString() => $"{ListType},{Category},{Name}";

        public ListItem(ListType listType, string category, string name)
        {
            ListType = listType;
            Category = category;
            Name = name;
        }
    }
}
