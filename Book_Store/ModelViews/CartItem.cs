using System;
using Book_Store.Models;

namespace Book_Store.ModelViews
{
    public class CartItem
    {
        public Product? product { get; set; }
        public int amount { get; set; }
        public double TotalMoney
        {
            get
            {
                if (product != null && product.Discount.HasValue)
                {
                    return amount * (product.Price.Value - product.Price.Value * product.Discount.Value / 100);
                }
                else
                {
                    return amount * product.Price.Value;
                }
            }
        }
    }
}
