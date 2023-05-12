using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using Book_Store.Extension;
using Book_Store.Models;
using Book_Store.ModelViews;
using NToastNotify;
using Book_Store.Areas.Admin.Models.Authentication;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static NuGet.Packaging.PackagingConstants;
using System.Net.NetworkInformation;
using System.Drawing.Printing;

namespace Book_Store.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authentication]
    //[Authorize]
    public class AdminOrdersController : Controller
    {
        private readonly BookManagementContext _context;
        private readonly IToastNotification _toastNotification;

        public AdminOrdersController(BookManagementContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }

        // GET: Admin/AdminOrders

        public IActionResult Index(int? page, int IsPaid = 0)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 20;
            var Orders = _context.Orders.Include(o => o.Customer).Include(o => o.TransactStatus)
                .AsNoTracking()
                .OrderBy(x => x.OrderDate).AsQueryable();

            if (IsPaid == 1)
            {
                Orders = Orders.Where(x => x.Paid == true);
            }
            else if (IsPaid == 2)
            {
                Orders = Orders.Where(x => x.Paid == false);
            }

            PagedList<Order> models = new PagedList<Order>(Orders, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.CurrentPaid = IsPaid;

            List<SelectListItem> lsStatus = new List<SelectListItem>();
            lsStatus.Add(new SelectListItem() { Text = "Đã Thanh Toán", Value = "1" });
            lsStatus.Add(new SelectListItem() { Text = "Chưa Thanh Toán", Value = "2" });
            foreach (var item in lsStatus)
            {
                if (item.Value == IsPaid.ToString())
                {
                    item.Selected = true;
                    break;
                }
            }
            ViewData["lsStatus"] = lsStatus;

            return View(models);
        }

        public IActionResult Filter(int IsPaid = 0)
        {
            var url = $"/Admin/AdminOrders/Index?IsPaid={IsPaid}";
            if (IsPaid == 0)
            {
                url = $"/Admin/AdminOrders/Index";
            }
            return Json(new { status = "success", redirectUrl = url });
        }


        // GET: Admin/AdminOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.TransactStatus)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            var Chitietdonhang = _context.OrderDetails
                .Include(x => x.Product)
                .AsNoTracking()
                .Where(x => x.OrderId == order.OrderId)
                .OrderBy(x => x.OrderDetailId)
                .ToList();
            ViewBag.ChiTiet = Chitietdonhang;
            return View(order);
        }


        public async Task<IActionResult> ChangeStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Customer)
                .FirstOrDefaultAsync(x => x.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["TrangThai"] = new SelectList(_context.TransactStatuses, "TransactStatusId", "Status", order.TransactStatusId);
            return PartialView("ChangeStatus", order);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int id, [Bind("OrderId,CustomerId,OrderDate,ShipDate,TransactStatusId,Deleted,Paid,PaymentDate,TotalMoney,PaymentId,Note,Address,LocationId,District,Ward")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var donhang = await _context.Orders.AsNoTracking().Include(x => x.Customer).FirstOrDefaultAsync(x => x.OrderId == id);
                    if (donhang != null)
                    {
                        donhang.Paid = order.Paid;
                        donhang.Deleted = order.Deleted;
                        donhang.TransactStatusId = order.TransactStatusId;
                        if (donhang.Paid == true|| donhang.TransactStatusId == 3)
                        {
                            donhang.PaymentDate = DateTime.Now;
                        }
                        if (donhang.TransactStatusId == 4) donhang.Deleted = true;
                        if (donhang.TransactStatusId == 2) donhang.ShipDate = DateTime.Now;
                        if (donhang.TransactStatusId == 5 || donhang.TransactStatusId == 4)
                        {
                            // Cập nhật tồn kho sản phẩm nếu đơn hàng bị hủy
                            var orderDetails = await _context.OrderDetails.Where(od => od.OrderId == order.OrderId).ToListAsync();
                            foreach (var detail in orderDetails)
                            {
                                // Lấy sản phẩm từ chi tiết đơn hàng
                                var product = await _context.Products.FindAsync(detail.ProductId);

                                // Cập nhật tồn kho của sản phẩm
                                product.UnitslnStock += detail.Amount;

                                _context.Products.Update(product);
                            }

                            await _context.SaveChangesAsync();
                        }
                    }
                    _context.Update(donhang);
                    await _context.SaveChangesAsync();
                    _toastNotification.AddSuccessToastMessage("Cập nhật trạng thái đơn hàng thành công");

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            ViewData["TrangThai"] = new SelectList(_context.TransactStatuses, "TransactStatusId", "Status", order.TransactStatusId);
            return PartialView("ChangeStatus", order);
        }

        // GET: Admin/AdminOrders/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId");
            ViewData["TransactStatusId"] = new SelectList(_context.TransactStatuses, "TransactStatusId", "TransactStatusId");
            return View();
        }

        // POST: Admin/AdminOrders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,CustomerId,OrderDate,ShipDate,TransactStatusId,Deleted,Paid,PaymentDate,TotalMoney,PaymentId,Note,Address,LocationId,District,Ward")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
            ViewData["TransactStatusId"] = new SelectList(_context.TransactStatuses, "TransactStatusId", "TransactStatusId", order.TransactStatusId);
            return View(order);
        }

        // GET: Admin/AdminOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
            ViewData["TransactStatusId"] = new SelectList(_context.TransactStatuses, "TransactStatusId", "TransactStatusId", order.TransactStatusId);
            return View(order);
        }

        // POST: Admin/AdminOrders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,CustomerId,OrderDate,ShipDate,TransactStatusId,Deleted,Paid,PaymentDate,TotalMoney,PaymentId,Note,Address,LocationId,District,Ward")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerId", order.CustomerId);
            ViewData["TransactStatusId"] = new SelectList(_context.TransactStatuses, "TransactStatusId", "TransactStatusId", order.TransactStatusId);
            return View(order);
        }

        // GET: Admin/AdminOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.TransactStatus)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            var Chitietdonhang = _context.OrderDetails
                .Include(x => x.Product)
                .AsNoTracking()
                .Where(x => x.OrderId == order.OrderId)
                .OrderBy(x => x.OrderDetailId)
                .ToList();
            ViewBag.ChiTiet = Chitietdonhang;

            return View(order);
        }

        // POST: Admin/AdminOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Orders == null)
            {
                return Problem("Entity set 'QlBansachContext.Accounts'  is null.");
            }
            var order = await _context.Orders.FindAsync(id);
            var table = _context.Orders.Include(t => t.OrderDetails).FirstOrDefault(t => t.OrderId == id);
            foreach (var item in table.OrderDetails)
            {
                _context.OrderDetails.Remove(item);
            }
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            _toastNotification.AddSuccessToastMessage("Xóa đơn hàng thành công");
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
