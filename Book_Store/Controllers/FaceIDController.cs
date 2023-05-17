using Book_Store.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NToastNotify;
using System;
using System.Diagnostics;
using System.Security.Claims;

namespace Book_Store.Controllers
{
    public class FaceIDController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly BookManagementContext _context;
        private readonly IToastNotification _toastNotification;
        public FaceIDController(IWebHostEnvironment webHostEnvironment, BookManagementContext context, IToastNotification toastNotification)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
            _toastNotification = toastNotification;
        }
        [Route("FaceID.html", Name = "FaceID")]
        [AllowAnonymous]
        public IActionResult Login()
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
        [Route("FaceID.html", Name = "FaceID")]
        [Route("api/checkFaceID")]
        public async Task<IActionResult> Login(string email, string imageData, string? returnUrl,int failedAttempts)
        {
            var imageDataBytes = Convert.FromBase64String(imageData.Substring("data:image/png;base64,".Length));
            var face = new Face
            {
                CheckFaceImg = imageDataBytes
            };

            var existingFace = _context.Faces.FirstOrDefault(x => x.Email == email);
            var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.Email.Trim() == email);
         


            if (existingFace != null)
            {
                // Đã tìm thấy đối tượng Face với email trùng khớp, cập nhật CheckFaceImg
                existingFace.CheckFaceImg = face.CheckFaceImg;
                _context.SaveChanges();
                string pythonPath = "C:/Users/PC/AppData/Local/Programs/Python/Python37/python.exe";
                string pythonScript = "C:/Users/PC/source/repos/web-net/AI/test1.py " + email;

                // Configure the process start info
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = pythonPath;
                startInfo.Arguments = pythonScript;
                Process process = new Process();
                //psi.Arguments = script;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                process.StartInfo = startInfo;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                process.Dispose();
                ViewBag.kq = output;

                if (output.Contains("yes"))
                {
                    HttpContext.Session.SetString("CustomerId", khachhang.CustomerId.ToString());
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
                        return Json(new { success = true, url = Url.Action("Dashboard", "Accounts") });
                    }
                    else
                    {
                        return Redirect(returnUrl);
                    }
                }
                else
                {
                    failedAttempts++;

                    if (failedAttempts < 3)
                    {
                        var re = 3 - failedAttempts;
                        // Allow the user to retry face recognition
                        _toastNotification.AddErrorToastMessage("Nhận diện không đúng. Số lần thử còn lại là " + re + " lần!");
                        return Json(new { success = true, url = Url.RouteUrl("FaceID") });
                    }
                    else
                    {
                        // Redirect the user to the login page
                        _toastNotification.AddErrorToastMessage("Nhận diện không đúng. Vui lòng đăng nhập bằng phương thức khác.");
                        return Json(new { success = true, url = Url.RouteUrl("DangNhap") });
                    }
                }

                // Tiến hành đăng nhập thành công
                // Chuyển hướng đến action "Dashboard" của controller "Account"

            }
            else
            {
                // Không tìm thấy đối tượng Face với email trùng khớp
                // Xử lý logic khi đăng nhập không thành công
                // Ví dụ: Hiển thị thông báo lỗi và chuyển hướng về trang đăng nhập
                _toastNotification.AddErrorToastMessage("Nhận diện không đúng");
                return Json(new { success = true, url = Url.RouteUrl("DangNhap") });
            }
        }


        [Route("RegisterFaceID.html", Name = "RegisterFaceID")]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [Route("api/checkRegisterFaceID")]
        public IActionResult RegisterSaveImage(string imageData)
        {
            var imageDataBytes = Convert.FromBase64String(imageData.Substring("data:image/png;base64,".Length));

            var taikhoanID = HttpContext.Session.GetString("CustomerId");
            var customer = _context.Customers.SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));

            if (customer != null)
            {
                var existingFace = _context.Faces.SingleOrDefault(x => x.Email == customer.Email);

                if (existingFace != null)
                {
                    existingFace.FaceImg = imageDataBytes;
                    _context.Faces.Update(existingFace);
                }
                else
                {
                    var newFace = new Face
                    {
                        FaceImg = imageDataBytes,
                        Email = customer.Email
                    };
                    _context.Faces.Add(newFace);
                }
                _toastNotification.AddSuccessToastMessage("Đăng Ký Thành Công");
                _context.SaveChanges();

            }

            return Json(new { success = true, url = Url.Action("Dashboard", "Accounts") });
        }

    }
}
