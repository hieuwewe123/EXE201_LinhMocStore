using System;
using System.ComponentModel.DataAnnotations;

namespace EXE201_LinhMocStore.Models;

public partial class Blog
{
    public int BlogId { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Image { get; set; }

    public string? Author { get; set; }

    public string? Summary { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsActive { get; set; }
}
