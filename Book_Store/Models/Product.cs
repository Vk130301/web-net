using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Book_Store.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public string? ShortDesc { get; set; }

    public string? Description { get; set; }

    public int? CateId { get; set; }

    public int? Price { get; set; }

    [Range(0, 100, ErrorMessage = "Phải từ 0 đến 100.")]
    public int? Discount { get; set; }


   
    public string? Thumb { get; set; }

    public DateTime? DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public bool BestSellers { get; set; }

    public bool HomeFlag { get; set; }

    public bool Active { get; set; }

    public string? Tags { get; set; }

    public string? Title { get; set; }

    public string? Alias { get; set; }

    public string? MetaDesc { get; set; }

    public string? MetaKey { get; set; }

    [Required(ErrorMessage = "Số lượng tồn kho bắt buộc nhập.")]
    [Range(0, 1000, ErrorMessage = "Số lượng tồn kho phải từ 0 đến 1000.")]
    public int? UnitslnStock { get; set; }

    public int? OriginalPrice { get; set; }

    public int? AuthorId { get; set; }

    public virtual Author? Author { get; set; }

    public virtual Category? Cate { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
