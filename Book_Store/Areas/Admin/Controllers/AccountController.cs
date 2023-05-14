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
using NToastNotify;
using Book_Store.Areas.Admin.Models;
using System.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Book_Store.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class AccountController : Controller
    {
        private readonly BookManagementContext _context;
        private readonly IToastNotification _toastNotification;

        public AccountController(BookManagementContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }
        // GET: /<controller>/  

        [HttpGet]
        public IActionResult AdminLogin(string? returnUrl = null)
        {
            var taikhoanID = HttpContext.Session.GetString("AccountId");
            if (taikhoanID != null)
            {
                return RedirectToAction("Index", "Home", new { Area = "Admin" });
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AdminLogin(LoginViewModel model, string? returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var admin = _context.Accounts.AsNoTracking().Include(x => x.Role).SingleOrDefault(p => p.Email.ToLower() == model.UserName.ToLower().Trim());
                    if (admin == null)
                    {
                        ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác";
                        return View(model);
                    }
                    string pass = (model.Password + admin.Salt.Trim()).ToMD5();
                    // + kh.Salt.Trim()
                    if (admin.Password != pass)
                    {
                        ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác";
                        return View(model);
                    }

                    if (admin.Active == false)
                    {
                        ViewBag.Error = "Tài khoản đã bị khóa";
                        return View(model);
                    }
                    //đăng nhập thành công

                    //ghi nhận thời gian đăng nhập
                    admin.LastLogin = DateTime.Now;
                    _context.Update(admin);
                    await _context.SaveChangesAsync();


                    //identity
                    //luuw seccion Makh
                    HttpContext.Session.SetString("AccountId", admin.AccountId.ToString());
                    HttpContext.Session.SetString("RoleName", admin.Role.RoleName.ToString());
                    var taikhoanID = HttpContext.Session.GetString("AccountId");

                    //identity
                    var userClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, admin.FullName),
                        new Claim(ClaimTypes.Email, admin.Email),
                        new Claim("AccountId", admin.AccountId.ToString()),
                        new Claim("RoleId", admin.RoleId.ToString()),
                        new Claim(ClaimTypes.Role, admin.Role.Description)
                    };
                    var grandmaIdentity = new ClaimsIdentity(userClaims, "User Identity");
                    var userPrincipal = new ClaimsPrincipal(new[] { grandmaIdentity });
                    await HttpContext.SignInAsync(userPrincipal);

                    //Identity
                    var claims = new List<Claim>
                    {   
                        new Claim(ClaimTypes.Name, admin.FullName),
                        new Claim(ClaimTypes.Email, admin.Email),
                        new Claim("AccountId", admin.AccountId.ToString()),
                        new Claim("RoleId", admin.RoleId.ToString()),
                        new Claim(ClaimTypes.Role, admin.Role.Description)
                    };

                    var iden = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(iden);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = model.RememberMe,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                        });
                    _toastNotification.AddSuccessToastMessage("Đăng nhập thành công");
                    if (string.IsNullOrEmpty(returnUrl))
                    {

                        return RedirectToAction("Index", "Home", new { Area = "Admin" });
                    }
                    else
                    {
                        return Redirect(returnUrl);
                    }
                }
            }
            catch
            {
                return RedirectToAction("AdminLogin", "Account", new { Area = "Admin" });
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult>AdminLogout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("AccountId");
            return RedirectToAction("AdminLogin", "Account", new { Area = "Admin" });
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }


        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            try
            {
                var taikhoanID = HttpContext.Session.GetString("AccountId");
                if (ModelState.IsValid)
                {
                    var taikhoan = _context.Accounts    .Find(Convert.ToInt32(taikhoanID));
                    if (taikhoan == null) return RedirectToAction("AdminLogin", "Account");

                    var passNow = (model.PasswordNow.Trim() + taikhoan.Salt.Trim()).ToMD5();

                    if (passNow == taikhoan.Password)
                    {
                        var passNew = (model.Password.Trim() + taikhoan.Salt.Trim()).ToMD5();
                        taikhoan.Password = passNew;
                        _context.Update(taikhoan);
                        _context.SaveChanges();
                        _toastNotification.AddSuccessToastMessage("Đổi mật khẩu thành công");
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        _toastNotification.AddErrorToastMessage("Mật khẩu cũ không chính xác");
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    _toastNotification.AddErrorToastMessage("Dữ liệu không hợp lệ");
                    return RedirectToAction("Index", "Home");
                }
            }
            catch
            {
                _toastNotification.AddErrorToastMessage("Thay đổi mật khẩu không thành công");
                return RedirectToAction("Index", "Home");
            }
        }

    }
}
