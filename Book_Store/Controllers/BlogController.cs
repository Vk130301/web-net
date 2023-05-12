using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Book_Store.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NToastNotify;
using PagedList.Core;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Book_Store.Controllers
{
    public class BlogController : Controller
    {
        private readonly BookManagementContext _context;
        private readonly IToastNotification _toastNotification;

        public BlogController(BookManagementContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }
        // GET: /<controller>/
        [Route("blogs.html",Name =("Blog"))]
        public IActionResult Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 10;
            var lsTinDangs = _context.Posts
                .AsNoTracking()
                .OrderBy(x => x.PostId);
            PagedList<Post> models = new PagedList<Post>(lsTinDangs, pageNumber, pageSize);

            ViewBag.CurrentPage = pageNumber;
            return View(models);
        }
        [Route("/tin-tuc/{Alias}-{id}.html",Name ="TinChiTiet")]
        public IActionResult Details(int id)
        {
            var tindang = _context.Posts.AsNoTracking().SingleOrDefault(x => x.PostId == id);
            if (tindang == null)
            {
                return RedirectToAction("Index");
            }
            var lsBaivietlienquan = _context.Posts
                .AsNoTracking()
                .Where(x => x.Published == true && x.PostId != id)
                .Take(3)
                .OrderByDescending(x => x.CreateDate).ToList();
            ViewBag.Baivietlienquan = lsBaivietlienquan;
            return View(tindang);
        }
    }
}
