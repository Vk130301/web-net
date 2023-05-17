using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Book_Store.Extension;
using Book_Store.Helpper;
using Book_Store.Models;
using Book_Store.ModelViews;
using NToastNotify;
using Microsoft.AspNetCore.Mvc.Routing;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Book_Store.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly BookManagementContext _context;
        private readonly IToastNotification _toastNotification;

        public AccountsController(BookManagementContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult ValidatePhone(string Phone)
        {
            try
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Phone.ToLower() == Phone.ToLower());
                if (khachhang != null)
                    return Json(data: "Số điện thoại : " + Phone + "đã được sử dụng");

                return Json(data: true);
                
            }
            catch
            {
                return Json(data: "Số điện thoại : " + Phone + "đã được sử dụng");
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult ValidateEmail(string Email)
        {
            try
            {
                var khachhang =  _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.ToLower() == Email.ToLower());
                if (khachhang != null)
                    return Json(data: "Email : " + Email + " đã được sử dụng");
                return Json(data: true);
            }
            catch
            {
                return Json(data: "Email : " + Email + " đã được sử dụng");
            }
        }
        [Route("tai-khoan-cua-toi.html", Name = "Dashboard")]
        public IActionResult Dashboard()
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                if (khachhang != null)
                {
                    var lsDonHang = _context.Orders
                        .Include(x => x.TransactStatus)
                        .AsNoTracking()
                        .Where(x => x.CustomerId == khachhang.CustomerId)
                        .OrderByDescending(x => x.OrderDate)
                        .ToList();
                    ViewBag.DonHang = lsDonHang;
                    return View(khachhang);
                }
                    
            }
            return RedirectToAction("Login");
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("dang-ky.html",Name ="DangKy")]
        public IActionResult DangkyTaiKhoan()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("dang-ky.html",Name ="DangKy")]
        public async Task<IActionResult> DangkyTaiKhoan(RegisterViewModel taikhoan)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    // Kiểm tra email và số điện thoại duy nhất
                   
                    string salt = Utilities.GetRandomKey();
                    Customer khachhang = new Customer
                    {
                        FullName = taikhoan.FullName,
                        Phone = taikhoan.Phone.Trim().ToLower(),
                        Email = taikhoan.Email.Trim().ToLower(),
                        Password = (taikhoan.Password + salt.Trim()).ToMD5(),
                        Active = true,
                        Salt = salt,
                        CreateDate = DateTime.Now
                    };
                    try
                    {
                        _context.Add(khachhang);
                        await _context.SaveChangesAsync();
                        //Lưu Session MaKh
                        HttpContext.Session.SetString("CustomerId", khachhang.CustomerId.ToString());

                        //Identity
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name,khachhang.FullName),
                            new Claim("CustomerId", khachhang.CustomerId.ToString())
                        };
                        ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        await HttpContext.SignInAsync(claimsPrincipal);
                        _toastNotification.AddSuccessToastMessage("Đăng ký thành công");
                        return RedirectToAction("Dashboard", "Accounts");
                    }
                    catch 
                    {
                        return RedirectToAction("DangkyTaiKhoan", "Accounts");
                    }
                }
                else
                {
                    return View(taikhoan);
                }
            }
            catch 
            {
                return View(taikhoan);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name ="DangNhap")]
        public IActionResult Login(string? returnUrl = null)
        {
            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            if (taikhoanID != null)
            {
                return RedirectToAction("Dashboard", "Accounts");   
            }
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("dang-nhap.html", Name ="DangNhap")]
        [Route("api/login")]
        public async Task<IActionResult> Login(LoginViewModel customer, string? returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool isEmail = Utilities.IsValidEmail(customer.UserName);
                    if (!isEmail) return BadRequest(); ;

                    var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.Trim() == customer.UserName);

                    if (khachhang == null)
                    {
                        _toastNotification.AddErrorToastMessage("Thông tin đăng nhập chưa chính xác!");
                        return BadRequest();
                    }      
                    string pass = (customer.Password + khachhang.Salt.Trim()).ToMD5();
                    if(khachhang.Password != pass)
                    {
                        _toastNotification.AddErrorToastMessage("Sai mật khẩu");
                        return BadRequest();
                    }
                    //kiem tra xem account co bi disable hay khong

                    if (khachhang.Active == false)
                    {
                        _toastNotification.AddErrorToastMessage("Tài khoản đã bị khóa");
                        return BadRequest();
                    }

                    //Luu Session MaKh
                    HttpContext.Session.SetString("CustomerId", khachhang.CustomerId.ToString());

                    //Identity
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, khachhang.FullName),
                        new Claim("CustomerId", khachhang.CustomerId.ToString())
                    };
                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(claimsPrincipal);
                    if (string.IsNullOrEmpty(returnUrl))
                    {
                        _toastNotification.AddSuccessToastMessage("Đăng nhập thành công");
                        return Json(new { success = true, url = Url.Action("Dashboard", "Accounts") }) ;
                    }
                    else
                    {
                        return Redirect(returnUrl);
                    }
                }
            }
            catch
            {
                return RedirectToAction("DangkyTaiKhoan", "Accounts");
            }
            return BadRequest();
        }
        [HttpGet]
        [Route("dang-xuat.html",Name ="DangXuat")]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            HttpContext.Session.Remove("CustomerId");
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        //[HttpPost]
        //public IActionResult ChangePassword(ChangePasswordViewModel model)
        //{
        //    try
        //    {
        //        var taikhoanID = HttpContext.Session.GetString("CustomerId");
        //        if (ModelState.IsValid)
        //        {
        //            var taikhoan = _context.Customers.Find(Convert.ToInt32(taikhoanID));
        //            var pass = (model.PasswordNow.Trim() + taikhoan.Salt.Trim()).ToMD5();
        //            if (taikhoan == null) return RedirectToAction("Login", "Accounts");
        //            {
        //                string passnew = (model.Password.Trim() + taikhoan.Salt.Trim()).ToMD5();
        //                taikhoan.Password = passnew;
        //                _context.Update(taikhoan);
        //                _context.SaveChanges();
        //                _toastNotification.AddSuccessToastMessage("Đổi mật khẩu thành công");
        //                return RedirectToAction("Dashboard", "Accounts");
        //            }
        //        }
        //    }
        //    catch 
        //    {
        //        _toastNotification.AddSuccessToastMessage("Thay đổi mật khẩu không thành công");
        //        return RedirectToAction("Dashboard", "Accounts");
        //    }
        //    _toastNotification.AddErrorToastMessage("Thay đổi mật khẩu không thành công");
        //    return RedirectToAction("Dashboard", "Accounts");
        //}
        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("CustomerId");
                if (ModelState.IsValid)
                {
                    var taikhoan = _context.Customers.Find(Convert.ToInt32(taikhoanID));
                    if (taikhoan == null) return RedirectToAction("Login", "Accounts");

                    var passNow = (model.PasswordNow.Trim() + taikhoan.Salt.Trim()).ToMD5();

                    if (passNow == taikhoan.Password)
                    {
                        var passNew = (model.Password.Trim() + taikhoan.Salt.Trim()).ToMD5();
                        taikhoan.Password = passNew;
                        _context.Update(taikhoan);
                        _context.SaveChanges();
                        _toastNotification.AddSuccessToastMessage("Đổi mật khẩu thành công");
                        return RedirectToAction("Dashboard", "Accounts");
                    }
                    else
                    {
                        _toastNotification.AddErrorToastMessage("Mật khẩu cũ không chính xác");
                        return RedirectToAction("Dashboard", "Accounts");
                    }
                }
                else
                {
                    _toastNotification.AddErrorToastMessage("Dữ liệu không hợp lệ");
                    return RedirectToAction("Dashboard", "Accounts");
                }
            }
            catch
            {
                _toastNotification.AddErrorToastMessage("Thay đổi mật khẩu không thành công");
                return RedirectToAction("Dashboard", "Accounts");
            }
        }


        [HttpGet]
        [Route("api/getcustomerId")]
        public ActionResult GetCustomerId()
        {
            var customerid = User.FindFirst("CustomerId")?.Value; // Sử dụng ?. để tránh lỗi với User.FindFirst trả về null
            return Json(new { customerID = customerid });
        }
    }
}
