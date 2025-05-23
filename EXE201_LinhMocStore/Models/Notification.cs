using System;
using System.Collections.Generic;

namespace EXE201_LinhMocStore.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int? UserId { get; set; }

    public string? Content { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
