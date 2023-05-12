using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Book_Store.Models;
using Book_Store.Areas.Admin.Models.Authentication;
using NToastNotify;
using PagedList.Core;
using Book_Store.Helpper;

namespace Book_Store.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authentication]
    public class AdminAuthorsController : Controller
    {
        private readonly BookManagementContext _context;
        private readonly IToastNotification _toastNotification;

        public AdminAuthorsController(BookManagementContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }

        // GET: Admin/AdminAuthors
        public IActionResult Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var lsAuthors = _context.Authors
                .AsNoTracking()
                .OrderBy(x => x.AuthorId);
            PagedList<Author> models = new PagedList<Author>(lsAuthors, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }

        // GET: Admin/AdminAuthors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Authors == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .FirstOrDefaultAsync(m => m.AuthorId == id);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // GET: Admin/AdminAuthors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminAuthors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AuthorId,AuthorName,Description,Thumb,Alias,Title")] Author author, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (ModelState.IsValid)
            {
                //Xu ly Thumb
                if (fThumb != null)
                {
                    string extension = Path.GetExtension(fThumb.FileName);
                    string imageName = Utilities.SEOUrl(author.AuthorName) + extension;
                    author.Thumb = await Utilities.UploadFile(fThumb, @"author", imageName.ToLower());
                }
                if (string.IsNullOrEmpty(author.Thumb)) author.Thumb = "default.jpg";
                author.Alias = Utilities.SEOUrl(author.AuthorName);
                _context.Add(author);
                await _context.SaveChangesAsync();
                _toastNotification.AddSuccessToastMessage("Thêm mới thành công");
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        // GET: Admin/AdminAuthors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Authors == null)
            {
                return NotFound();
            }

            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        // POST: Admin/AdminAuthors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AuthorId,AuthorName,Description,Thumb,Alias,Title")] Author author, Microsoft.AspNetCore.Http.IFormFile fThumb)
        {
            if (id != author.AuthorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (fThumb != null)
                    {
                        string extension = Path.GetExtension(fThumb.FileName);
                        string imageName = Utilities.SEOUrl(author.AuthorName) + extension;
                        author.Thumb = await Utilities.UploadFile(fThumb, @"author", imageName.ToLower());
                    }
                    if (string.IsNullOrEmpty(author.Thumb)) author.Thumb = "default.jpg";
                    author.Alias = Utilities.SEOUrl(author.AuthorName);
                    _context.Update(author);
                    await _context.SaveChangesAsync();
                    _toastNotification.AddSuccessToastMessage("Chỉnh sửa thành công");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuthorExists(author.AuthorId))
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
            return View(author);
        }

        // GET: Admin/AdminAuthors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Authors == null)
            {
                return NotFound();
            }

            var author = await _context.Authors
                .FirstOrDefaultAsync(m => m.AuthorId == id);
            if (author == null)
            {
                return NotFound();
            }

            return View(author);
        }

        // POST: Admin/AdminAuthors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Authors == null)
            {
                return Problem("Entity set 'BookManagementContext.Authors'  is null.");
            }
            var author = await _context.Authors.FindAsync(id);
            var table = _context.Authors.Include(t => t.Products).FirstOrDefault(t => t.AuthorId == id);
            foreach (var item in table.Products)
            {
                _context.Products.Remove(item);
            }
            if (author != null)
            {
                _context.Authors.Remove(author);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuthorExists(int id)
        {
          return (_context.Authors?.Any(e => e.AuthorId == id)).GetValueOrDefault();
        }
    }
}
