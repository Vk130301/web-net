using System;
using System.Collections.Generic;

namespace Book_Store.Models;

public partial class Face
{
    public int FaceId { get; set; }

    public byte[]? FaceImg { get; set; }

    public byte[]? CheckFaceImg { get; set; }

    public string? Email { get; set; }
}
