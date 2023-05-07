using System;
using System.Collections.Generic;
using Book_Store.Models;

namespace Book_Store.ModelViews
{
    public class XemDonHang
    {
        public Order? DonHang { get; set; }
        public List<OrderDetail>? ChiTietDonHang { get; set; }
    }
}
