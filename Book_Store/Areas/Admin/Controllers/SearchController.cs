using Book_Store.Areas.Admin.Models.Authentication;
using Book_Store.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Book_Store.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authentication]
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
            ls = _context.Products
                    .AsNoTracking()
                    .Include(a => a.Cate)
                    .Include(a => a.Author)
                    .Where(x => x.ProductName.Contains(keyword) ||
                           (x.Cate.CateName.Contains(keyword) ||
                                 x.Author.AuthorName.Contains(keyword)))
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
