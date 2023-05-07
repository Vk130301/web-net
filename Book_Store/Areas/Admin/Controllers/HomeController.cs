using Book_Store.Areas.Admin.Models.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Book_Store.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {

        [Area("Admin")]
        [Authentication]
        public IActionResult Index()
        {
            return View();
        }
    }
}
