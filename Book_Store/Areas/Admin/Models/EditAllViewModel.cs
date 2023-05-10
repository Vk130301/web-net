using Book_Store.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Book_Store.Areas.Admin.Models
{
    public class EditAllViewModel
    {
        public List<Product>? SelectedProducts { get; set; }
        public int Discount { get; set; }
    }
}
