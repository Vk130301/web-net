using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Book_Store.Models;
using NToastNotify;
using PagedList.Core;
using Book_Store.Helpper;
using Book_Store.Areas.Admin.Models.Authentication;
using Book_Store.Areas.Admin.Models;
using Book_Store.Controllers;

namespace Book_Store.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authentication]
    public class AdminProductsController : Controller
    {
        private readonly BookManagementContext _context;
        private readonly IToastNotification _toastNotification;


        public AdminProductsController(BookManagementContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }


        // GET: Admin/AdminProducts
        public IActionResult Index(int page = 1, int CateID = 0, int AuthorID = 0)
        {
            var pageNumber = page;
            var pageSize = 20;

            List<Product> lsProducts = new List<Product>();
            if (CateID != 0)
            {
                if (AuthorID != 0)
                {
                    lsProducts = _context.Products
                        .AsNoTracking()
                        .Where(x => x.CateId == CateID && x.AuthorId == AuthorID)
                        .Include(x => x.Cate)
                        .Include(x => x.Author)
                        .OrderBy(x => x.ProductId)
                        .ToList();
                }
                else
                {
                    lsProducts = _context.Products
                        .AsNoTracking()
                        .Where(x => x.CateId == CateID)
                        .Include(x => x.Cate)
                        .Include(x => x.Author)
                        .OrderBy(x => x.ProductId)
                        .ToList();
                }
            }
            else
            {
                if (AuthorID != 0)
                {
                    lsProducts = _context.Products
                        .AsNoTracking()
                        .Where(x => x.AuthorId == AuthorID)
                        .Include(x => x.Cate)
                        .Include(x => x.Author)
                        .OrderBy(x => x.ProductId)
                        .ToList();
                }
                else
                {
                    lsProducts = _context.Products
                        .AsNoTracking()
                        .Include(x => x.Cate)
                        .Include(x => x.Author)
                        .OrderBy(x => x.ProductId)
                        .ToList();
                }
            }



            PagedList<Product> models = new PagedList<Product>(lsProducts.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentCateID = CateID;
            ViewBag.CurrentAuthorID = AuthorID;

            ViewBag.CurrentPage = pageNumber;

            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CateId", "CateName");
            ViewData["TacGia"] = new SelectList(_context.Authors, "AuthorId", "AuthorName");

            return View(models);
        }

        public IActionResult Filter(int CateID = 0, int AuthorID = 0)
        {
            var url = $"/Admin/AdminProducts/Index?CateID={CateID}&AuthorID={AuthorID}";
            if (CateID == 0 & AuthorID == 0)
            {
                url = $"/Admin/AdminProducts/Index";
            }
            else
            {
                if (AuthorID == 0) url = $"/Admin/AdminProducts/Index?CateID={CateID}";
                if (CateID == 0) url = $"/Admin/AdminProducts/Index?AuthorID={AuthorID}";
            }
            return Json(new { status = "success", redirectUrl = url });
        }


        // GET: Admin/AdminProducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Cate)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/AdminProducts/Create
        public IActionResult Create()
        {
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CateId", "CateName");
            ViewData["TacGia"] = new SelectList(_context.Authors, "AuthorId", "AuthorName");
            return View();
        }

        // POST: Admin/AdminProducts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,ShortDesc,Description,CateId,Price,Discount,Thumb,Video,DateCreated,DateModified,BestSellers,HomeFlag,Active,Tags,Title,Alias,MetaDesc,MetaKey,UnitslnStock,OriginalPrice")] Product product, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (ModelState.IsValid)
            {
                product.ProductName = Utilities.ToTitleCase(product.ProductName);
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string image = Utilities.SEOUrl(product.ProductName) + extension;
                    product.Thumb = await Utilities.UploadFile(fThumb, @"products", image.ToLower());
                }
                if (string.IsNullOrEmpty(product.Thumb)) product.Thumb = "default.jpg";
                product.Alias = Utilities.SEOUrl(product.ProductName);
                product.DateModified = DateTime.Now;
                product.DateCreated = DateTime.Now;

                _context.Add(product);
                await _context.SaveChangesAsync();
                _toastNotification.AddSuccessToastMessage("Thêm mới thành công");
                return RedirectToAction(nameof(Index));
            }
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CateId", "CateName", product.CateId);
            ViewData["TacGia"] = new SelectList(_context.Authors, "AuthorId", "AuthorName", product.AuthorId);
            return View(product);
        }

        // GET: Admin/AdminProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CateId", "CateName", product.CateId);
            ViewData["TacGia"] = new SelectList(_context.Authors, "AuthorId", "AuthorName", product.AuthorId);
            return View(product);
        }

        // POST: Admin/AdminProducts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,ShortDesc,Description,CateId,Price,Discount,Thumb,Video,DateCreated,DateModified,BestSellers,HomeFlag,Active,Tags,Title,Alias,MetaDesc,MetaKey,UnitslnStock,OriginalPrice")] Product product, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    product.ProductName = Utilities.ToTitleCase(product.ProductName);
                    if (fThumb != null)
                    {
                        string extension = Path.GetExtension(fThumb.FileName);
                        string image = Utilities.SEOUrl(product.ProductName) + extension;
                        product.Thumb = await Utilities.UploadFile(fThumb, @"products", image.ToLower());
                    }
                    if (string.IsNullOrEmpty(product.Thumb)) product.Thumb = "default.jpg";
                    product.Alias = Utilities.SEOUrl(product.ProductName);
                    product.DateModified = DateTime.Now;

                    _context.Update(product);
                    
                    await _context.SaveChangesAsync();
                    _toastNotification.AddSuccessToastMessage("Cập nhật thành công");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DanhMuc"] = new SelectList(_context.Categories, "CateId", "CateName", product.CateId);
            ViewData["TacGia"] = new SelectList(_context.Authors, "AuthorId", "AuthorName", product.AuthorId);
            return View(product);
        }

        [HttpPost]
        public ActionResult EditAll(string ids, int discount)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var productIds = ids.Split(',').Select(int.Parse).ToArray();
                var products = _context.Products.Where(p => productIds.Contains(p.ProductId)).ToList();
                if (products.Count > 0)
                {
                    foreach (var product in products)
                    {
                        product.Discount = discount;
                        _context.Products.Update(product);
                    }

                    _context.SaveChanges();
                    return Json(new { success = true });
                }
            }

            return Json(new { success = false });
        }


        // GET: Admin/AdminProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Cate)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/AdminProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'QlBansachContext.Products'  is null.");
            }
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }
            
            await _context.SaveChangesAsync();
            _toastNotification.AddSuccessToastMessage("Xóa thành công");
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
          return (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }
    }
}
