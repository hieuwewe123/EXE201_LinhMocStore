using System;
using System.Collections.Generic;

namespace EXE201_LinhMocStore.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    // Thông tin địa chỉ giao hàng
    public string? ReceiverName { get; set; }
    public string? ReceiverPhone { get; set; }
    public string? ShippingAddress { get; set; }
    public string? DeliveryNote { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User? User { get; set; }
}
