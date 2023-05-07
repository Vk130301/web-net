using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Book_Store.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using Book_Store.Extension;
using Book_Store.ModelViews;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Book_Store.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly QlBansachContext _context;
        private readonly IToastNotification _toastNotification;

        public ShoppingCartController(QlBansachContext context, IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }
        public List<CartItem> GioHang
        {
            get
            {
                var giohangJson = HttpContext.Session.GetString("GioHang");
                var giohang = !string.IsNullOrEmpty(giohangJson) ? JsonConvert.DeserializeObject<List<CartItem>>(giohangJson) : new List<CartItem>();
                return giohang;
            }
            set
            {
                var giohangJson = JsonConvert.SerializeObject(value);
                HttpContext.Session.SetString("GioHang", giohangJson);
            }
        }

        [HttpPost]
        [Route("api/cart/add")]
        public IActionResult AddToCart(int productID, int? amount)
        {
            List<CartItem> cart = GioHang;
            try
            {
                //Them san pham vao gio hang
                CartItem item = cart.SingleOrDefault(p => p.product.ProductId == productID && p.product.UnitslnStock > 0);
                if (item != null) // da co => cap nhat so luong
                {
                    // Sản phẩm đã có trong giỏ hàng
                    int totalAmount = item.amount + (amount.HasValue ? amount.Value : 1);
                    if (totalAmount > item.product.UnitslnStock)
                    {
                        _toastNotification.AddErrorToastMessage("Số lượng vượt quá số lượng tồn kho");
                        return Json(new { success = false });
                    }
                    else
                    {
                        // Số lượng sản phẩm trong giỏ hàng và số lượng mới được thêm vào không vượt quá tồn kho
                        item.amount = totalAmount;
                        //luu lai session
                        var gson = new JsonSerializerSettings();
                        var gioHangJson = JsonConvert.SerializeObject(cart, gson);
                        HttpContext.Session.SetString("GioHang", gioHangJson);
                    }
                }
                else
                {
                    Product hh = _context.Products.SingleOrDefault(p => p.ProductId == productID && p.UnitslnStock > 0);
                    if (hh != null)
                    {
                        // Kiểm tra số lượng sản phẩm mới được thêm vào có vượt quá tồn kho hay không
                        if (amount.HasValue && amount.Value > hh.UnitslnStock)
                        {
                            // Hiển thị thông báo lỗi
                            _toastNotification.AddErrorToastMessage("Số lượng vượt quá số lượng tồn kho");
                            return Json(new { success = false });
                        }
                        else
                        {
                            // Thêm sản phẩm vào giỏ hàng
                            item = new CartItem
                            {
                                amount = amount.HasValue ? amount.Value : 1,
                                product = hh
                            };
                            cart.Add(item);
                            //Luu lai Session
                            var gson = new JsonSerializerSettings();
                            var gioHangJson = JsonConvert.SerializeObject(cart, gson);
                            HttpContext.Session.SetString("GioHang", gioHangJson);
                        }
                    }
                    else
                    {
                        _toastNotification.AddErrorToastMessage("Sản phẩm đã hết số lượng");
                        return Json(new { success = false });
                    }
                }
                _toastNotification.AddSuccessToastMessage("Thêm sản phẩm thành công");
                return Json(new { success = true, gioHang = cart });

            }
            catch
            {
                return Json(new { success = false });
            }
        }
        [HttpPost]
        [Route("api/cart/checklogin")]
        public IActionResult CheckLogin(int[] productID, int[]? amount)
        {
            List<CartItem> cart = GioHang;
            for (int i = 0; i < productID.Length; i++)
            {
                var productId = productID[i];
                var product = _context.Products.SingleOrDefault(p => p.ProductId == productId);
                var cartItem = new CartItem { product = product, amount = amount?[i] ?? 1 };
                cart.Add(cartItem);
            }
            var gson = new JsonSerializerSettings();
            var gioHangJson = JsonConvert.SerializeObject(cart, gson);
            HttpContext.Session.SetString("GioHang", gioHangJson);
            return Json(new { success = true, gioHang = cart }); ;
        }

        [HttpPost]
        [Route("api/cart/update")]
        public IActionResult UpdateCart(int productID,int? amount)
        {
            //Lay gio hang ra de xu ly
            List<CartItem> cart = GioHang;
            try
            {
                if (cart != null)
                {
                    CartItem item = cart.SingleOrDefault(p => p.product.ProductId == productID);
                    if (item != null && amount.HasValue) // da co -> cap nhat so luong
                    {
                        item.amount = amount.Value;
                    }
                    Product hh = _context.Products.SingleOrDefault(p => p.ProductId == productID && p.UnitslnStock > 0);
                    if (amount.HasValue && amount.Value > hh.UnitslnStock)
                    {
                        // Hiển thị thông báo lỗi
                        _toastNotification.AddErrorToastMessage("Số lượng vượt quá số lượng tồn kho");
                        return Json(new { success = false });
                    }
                    //Luu lai session
                    var gson = new JsonSerializerSettings();
                    var gioHangJson = JsonConvert.SerializeObject(cart, gson);
                    HttpContext.Session.SetString("GioHang", gioHangJson);
                }
                return Json(new { success = true, gioHang = cart });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [Route("api/cart/remove")]
        public ActionResult Remove(int productID)
        {
            
            try
            {
                List<CartItem> cart = GioHang;
                CartItem item = cart.SingleOrDefault(p => p.product.ProductId == productID);
                if (item != null)
                {
                    cart.Remove(item);
                }
                //luu lai session
                var gson = new JsonSerializerSettings();
                var gioHangJson = JsonConvert.SerializeObject(cart, gson);
                HttpContext.Session.SetString("GioHang", gioHangJson);
                return Json(new { success = true, gioHang = cart });
            }
            catch 
            {
                return Json(new { success = false });
            }
        }
        [HttpPost]
        [Route("api/cart/removelocalstorage")]
        public ActionResult CheckRemove()
        {
            try
            {
                List<CartItem> cart = GioHang;
                return Json(new { success = true, gioHang = cart });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
        [Route("cart.html",Name ="Cart")]
        public IActionResult Index()
        {
            return View(GioHang);
        }
    }
}
