using System;
using System.Collections.Generic;
using Book_Store.Models;

namespace Book_Store.ModelViews
{
    public class ProductHomeVM
    {
        public Category? category { get; set; }
        public List<Product>? lsProducts { get; set; }
    }
}
