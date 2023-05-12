using System;
using System.Collections.Generic;

namespace Book_Store.Models;

public partial class Author
{
    public int AuthorId { get; set; }

    public string? AuthorName { get; set; }

    public string? Description { get; set; }

    public string? Thumb { get; set; }

    public string? Alias { get; set; }

    public string? Title { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
