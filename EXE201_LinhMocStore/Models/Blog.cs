using System;
using System.Collections.Generic;

namespace EXE201_LinhMocStore.Models;

public partial class Blog
{
    public int BlogId { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public DateTime CreatedAt { get; set; }
}
