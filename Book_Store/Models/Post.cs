using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Book_Store.Models;

public partial class Post
{
    public int PostId { get; set; }

    public string? Title { get; set; }

    public string? Scontents { get; set; }

    public string? Contents { get; set; }

    [Required(ErrorMessage = "Bắt buộc nhập hình ảnh.")]
    public string? Thumb { get; set; }

    public bool Published { get; set; }

    public string? Alias { get; set; }

    public DateTime? CreateDate { get; set; }

    public string? Author { get; set; }

    public int? AccountId { get; set; }

    public string? Tags { get; set; }

    public int? CateId { get; set; }

    public bool IsHot { get; set; }

    public bool IsNewfeed { get; set; }

    public string? MetaKey { get; set; }

    public string? MetaDesc { get; set; }

    public int? Views { get; set; }

    public virtual Account? Account { get; set; }

    public virtual Category? Cate { get; set; }
}
