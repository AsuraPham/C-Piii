using projectDemo.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace projectDemo.Data.IReponsitory
{
   public interface ICategoryReponsitory
    {
        IEnumerable<Category> Categories { get; }
    }
}
