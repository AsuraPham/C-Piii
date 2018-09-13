using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace projectDemo.Data.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public List<Drink> Drinks { get; set; }
    }
}
