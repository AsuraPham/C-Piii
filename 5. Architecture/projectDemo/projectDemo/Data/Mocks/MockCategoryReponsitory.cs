using projectDemo.Data.IReponsitory;
using projectDemo.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace projectDemo.Data.Mocks
{
    public class MockCategoryReponsitory : ICategoryReponsitory
    {
        public IEnumerable<Category> Categories
        {
            get
            {
                return new List<Category> {
                   new Category { CategoryName = "HHHHHHH", CategoryDescription = "hhihihi" },
                   new Category { CategoryName = "KKKKKKK", CategoryDescription = "hahahah" }
            };
            }
        }
    }
}
