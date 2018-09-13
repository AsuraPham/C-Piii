using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using projectDemo.Data.IReponsitory;

namespace projectDemo.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryReponsitory _categoryReponsitory;
        public CategoryController(ICategoryReponsitory categoryReponsitory)
        {
            _categoryReponsitory = categoryReponsitory;
        }
        public ViewResult List()
        {
            var category = _categoryReponsitory.Categories;
            return View(category);
        }
    }
}