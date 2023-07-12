using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNZApp
{
    public class ListItem
    {
        public ListType listType { get; set; }
        private string Category;
        public string category
        {
            get { return Category; }
            set { Category = value.ToLower(); }
        }
        private string Name;
        public string name
        {
            get { return Name; }
            set { Name = value.ToLower(); }
        }
        public string formattedName
        {
            get
            {
                return name.ToUpper();
            }
        }
        public string formattedCategory
        { 
            get
            {
                string category = Category;
                category = char.ToUpper(category[0]) + category.Substring(1);
                return category;
            } 
        }
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
