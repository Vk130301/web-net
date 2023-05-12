using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Book_Store.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Book_Store.ModelViews;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Book_Store.Controllers
{
    public class SearchController : Controller
    {
        private readonly BookManagementContext _context;

        public SearchController(BookManagementContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult FindProduct(string keyword)
        {
            List<Product> ls = new List<Product>();
            if (string.IsNullOrEmpty(keyword) || keyword.Length < 1)
            {
                return PartialView("ListProductsSearchPartial", null);
            }
            ls = _context.Products.AsNoTracking()
                                  .Include(a => a.Cate)
                                  .Where(x => x.ProductName.Contains(keyword))
                                  .OrderByDescending(x => x.ProductName)
                                  .Take(10)
                                  .ToList();
            if (ls == null)
            {
                return PartialView("ListProductsSearchPartial", null);
            }
            else
            {
                return PartialView("ListProductsSearchPartial", ls);
            }
        }
    }
}
