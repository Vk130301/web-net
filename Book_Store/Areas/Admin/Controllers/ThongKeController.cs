﻿using Book_Store.Areas.Admin.Models.Authentication;
using Book_Store.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using NToastNotify;
using System.Text;

namespace Book_Store.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authentication]
    public class ThongKeController : Controller
    {
        private readonly QlBansachContext _context;

        public ThongKeController(QlBansachContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetStatistical(string fromDate, string toDate)
        {
            var query = from o in _context.Orders
                        join od in _context.OrderDetails on o.OrderId equals od.OrderId
                        join p in _context.Products on od.ProductId equals p.ProductId
                        where o.PaymentDate != null
                        select new
                        {
                            PaymentDate = o.PaymentDate,
                            Amount = od.Amount,
                            Price = od.Price,
                            OriginalPrice = p.OriginalPrice
                        };
            if (!string.IsNullOrEmpty(fromDate))
            {
                DateTime startDate = DateTime.ParseExact(fromDate, "dd/MM/yyyy", null);
                query = query.Where(x => x.PaymentDate.Value >= startDate && x.PaymentDate.Value <= startDate.AddDays(1).AddSeconds(-1));
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                DateTime endDate = DateTime.ParseExact(toDate, "dd/MM/yyyy", null);
                query = query.Where(x => x.PaymentDate.Value >= endDate && x.PaymentDate.Value <= endDate.AddDays(1).AddSeconds(-1));
            }

            var result = query.GroupBy(x => x.PaymentDate.Value)
                            .Select(x => new
                            {
                                Date = x.Key.ToString("dd/MM/yyyy"),
                                ToTalBuy = x.Sum(y => y.Amount * y.OriginalPrice),
                                ToTalSell = x.Sum(y => y.Amount * y.Price),
                                FirstOrderDate = x.Min(y => y.PaymentDate),
                                LastOrderDate = x.Max(y => y.PaymentDate)
                            })
                            .Select(x => new
                            {
                                Date = x.Date,
                                DoanhThu = x.ToTalSell,
                                LoiNhuan = x.ToTalSell - x.ToTalBuy
                            });

            return Json(new { Data = result });
        }

    }
}
