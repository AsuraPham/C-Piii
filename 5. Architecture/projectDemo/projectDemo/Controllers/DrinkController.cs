using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using projectDemo.Data.IReponsitory;

namespace projectDemo.Controllers
{
    public class DrinkController : Controller
    {
        private readonly ICategoryReponsitory _categoryReponsitory;
        private readonly IDrinksReponsitory _drinksReponsitory;
        public DrinkController(IDrinksReponsitory drinksReponsitory, ICategoryReponsitory categoryReponsitory)
        {
            _categoryReponsitory = categoryReponsitory;
            _drinksReponsitory = drinksReponsitory;
        }
        public IActionResult List()
        {
            var drink = _drinksReponsitory.Drinks;
            return View(drink);
        }
    }
}