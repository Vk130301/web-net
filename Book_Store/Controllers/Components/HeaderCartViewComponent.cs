using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Book_Store.Extension;
using Book_Store.ModelViews;
using Newtonsoft.Json;

namespace Book_Store.Controllers.Components
{
    public class HeaderCartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var giohangJson = HttpContext.Session.GetString("GioHang");
            var cart = !string.IsNullOrEmpty(giohangJson) ? JsonConvert.DeserializeObject<List<CartItem>>(giohangJson) : new List<CartItem>();
            return View(cart);
        }
    }
}
