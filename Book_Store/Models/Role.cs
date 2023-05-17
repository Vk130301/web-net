using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Book_Store.Models;

public partial class Role
{
    public int RoleId { get; set; }

    [Required(ErrorMessage = "Bắt buộc nhập tên quyền truy cập.")]
    public string? RoleName { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
