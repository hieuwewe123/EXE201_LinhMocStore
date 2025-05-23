using System;
using System.Collections.Generic;

namespace EXE201_LinhMocStore.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public string? Description { get; set; }

    public string? Material { get; set; }

    public string? Size { get; set; }

    public string? Image { get; set; }

    public int? CategoryId { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
