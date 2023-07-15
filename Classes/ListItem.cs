namespace BNZApp
{
    public class ListItem
    {
        public ListType listType { get; set; }
        public string category { get { return Category; } set { Category = value.ToLower(); } }
        private string Category;
        public string name { get { return Name; } set { Name = value.ToLower(); } }
        private string Name;
        public string formattedName { get { return name.ToUpper(); } }
        public string formattedCategory { get { return char.ToUpper(Category[0]) + Category.Substring(1); } }

        public override string ToString()
        {
            return $"{listType},{category},{name}";
        }
        public ListItem(ListType listType, string category, string name) 
        {
            this.listType = listType;
            this.category = category;
            this.name = name; 
        }
    }
}
