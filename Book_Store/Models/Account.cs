using System;
using System.Collections.Generic;

namespace Book_Store.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Salt { get; set; }

    public bool Active { get; set; }

    public string? FullName { get; set; }

    public int? RoleId { get; set; }

    public DateTime? LastLogin { get; set; }

    public DateTime? CreateDate { get; set; }

    public int? FaceId { get; set; }

    public virtual Face? Face { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual Role? Role { get; set; }
}
