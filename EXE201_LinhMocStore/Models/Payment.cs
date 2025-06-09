using System;
using System.Collections.Generic;

namespace EXE201_LinhMocStore.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? OrderId { get; set; }

    public string? Content { get; set; }

    public decimal? Price { get; set; }

    public string? TransactionCode { get; set; }

    public string? Status { get; set; }
    public bool isCompleted {  get; set; }
    public DateTime? CreatedAt { get; set; }

    public virtual Order? Order { get; set; }
}
