using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Book_Store.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using PagedList.Core;


namespace Book_Store.Controllers
{
    public class ProductController : Controller
    {
        private readonly BookManagementContext _context;
        private readonly IToastNotification _toastNotification;

        public ProductController(BookManagementContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }
        [Route("shop.html", Name = ("ShopProduct"))]
        public IActionResult Index(int? page)
        {
            try
            {
                var pageNumber = page == null || page <= 0 ? 1 : page.Value;
                var pageSize = 10;
                var lsTinDangs = _context.Products
                    .AsNoTracking()
                    .OrderBy(x => x.DateCreated);
                PagedList<Product> models = new PagedList<Product>(lsTinDangs, pageNumber, pageSize);

                ViewBag.CurrentPage = pageNumber;
                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
            

        }
        [Route("danhmuc/{Alias}", Name = ("ListProduct"))]
        public IActionResult List(string Alias, int page = 1)
        {
            try
            {
                var pageSize = 10;
                var danhmuc = _context.Categories.AsNoTracking().SingleOrDefault(x => x.Alias == Alias);

                var lsTinDangs = _context.Products
                    .AsNoTracking()
                    .Where(x => x.CateId == danhmuc.CateId)
                    .OrderByDescending(x => x.DateCreated);
                PagedList<Product> models = new PagedList<Product>(lsTinDangs, page, pageSize);
                ViewBag.CurrentPage = page;
                ViewBag.CurrentCat = danhmuc;
                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }


        }

        [Route("tacgia/{Alias}", Name = ("ListProducts"))]
        public IActionResult ListAuthor(string Alias, int page = 1)
        {
            try
            {
                var pageSize = 10;
                var tacgia = _context.Authors.AsNoTracking().SingleOrDefault(x => x.Alias == Alias);

                var lsTinDangs = _context.Products
                    .AsNoTracking()
                    .Where(x => x.AuthorId == tacgia.AuthorId)
                    .OrderByDescending(x => x.DateCreated);
                PagedList<Product> models = new PagedList<Product>(lsTinDangs, page, pageSize);
                ViewBag.CurrentPage = page;
                ViewBag.CurrentCat = tacgia;
                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Route("/{Alias}-{id}.html", Name = ("ProductDetails"))]
        public IActionResult Details(int id)
        {
            try
            {
                var product = _context.Products.Include(x => x.Cate).Include(x=>x.Author).FirstOrDefault(x => x.ProductId == id);
                if (product == null)
                {
                    return RedirectToAction("Index");
                }
                var lsProduct = _context.Products
                    .AsNoTracking()
                    .Where(x => x.CateId == product.CateId && x.ProductId != id && x.Active == true)
                    .Take(4)
                    .OrderByDescending(x => x.DateCreated)
                    .ToList();
                ViewBag.SanPham = lsProduct;
                return View(product);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }


        }

    }
}
