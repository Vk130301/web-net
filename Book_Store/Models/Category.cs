using System;
using System.Collections.Generic;

namespace Book_Store.Models;

public partial class Category
{
    public int CateId { get; set; }

    public string? CateName { get; set; }

    public string? Description { get; set; }

    public int? Ordering { get; set; }

    public bool Published { get; set; }

    public string? Thumb { get; set; }

    public string? Title { get; set; }

    public string? Alias { get; set; }

    public string? MetaDesc { get; set; }

    public string? MetaKey { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
