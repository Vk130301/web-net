using System;
using System.Collections.Generic;
using Book_Store.Models;

namespace Book_Store.ModelViews
{
    public class HomeViewVM
    {
        public List<Post>? TinTucs { get; set; }
        public List<ProductHomeVM>? Products { get; set; }
    }
}
