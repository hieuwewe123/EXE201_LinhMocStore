using System;
using System.Collections.Generic;

namespace EXE201_LinhMocStore.Models;

public partial class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
