using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Book_Store.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using Book_Store.ModelViews;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Book_Store.Controllers
{
    public class DonHangController : Controller
    {
        private readonly BookManagementContext _context;
        private readonly IToastNotification _toastNotification;

        public DonHangController(BookManagementContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }
        [HttpPost]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            try
            {
                var taikhoanID = HttpContext.Session.GetString("CustomerId");
                if (string.IsNullOrEmpty(taikhoanID)) return RedirectToAction("Login", "Accounts");
                var khachhang = _context.Customers.AsNoTracking().SingleOrDefault(x => x.CustomerId == Convert.ToInt32(taikhoanID));
                if (khachhang == null) return NotFound();
                var donhang = await _context.Orders
                    .Include(x => x.TransactStatus)
                    .FirstOrDefaultAsync(m => m.OrderId == id && Convert.ToInt32(taikhoanID) == m.CustomerId);
                if (donhang == null) return NotFound();

                var chitietdonhang = _context.OrderDetails
                    .Include(x => x.Product)
                    .AsNoTracking()
                    .Where(x => x.OrderId == id)
                    .OrderBy(x => x.OrderDetailId)
                    .ToList();
                XemDonHang donHang = new XemDonHang();
                donHang.DonHang = donhang;
                donHang.ChiTietDonHang = chitietdonhang;
                return PartialView("Details", donHang);
                
            }
            catch 
            {
                return NotFound();
            }
        }
        [HttpPost]
        public async Task<ActionResult> HuyDonHang(int id)
        {
            // Lấy đơn hàng từ cơ sở dữ liệu dựa trên mã đơn hàng (id)
            var donHang = _context.Orders.Find(id);

            if (donHang != null)
            {
                // Cập nhật trạng thái của đơn hàng thành "Đã hủy"
                donHang.TransactStatusId = 4;
                var orderDetails = await _context.OrderDetails.Where(od => od.OrderId == id).ToListAsync();
                foreach (var detail in orderDetails)
                {
                    // Lấy sản phẩm từ chi tiết đơn hàng
                    var product = await _context.Products.FindAsync(detail.ProductId);

                    // Cập nhật tồn kho của sản phẩm
                    product.UnitslnStock += detail.Amount;

                    _context.Products.Update(product);
                }

                await _context.SaveChangesAsync();

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.SaveChanges();
                _toastNotification.AddSuccessToastMessage("Đã Hủy Đơn Hàng Thành Công");

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
        }

    }
}
