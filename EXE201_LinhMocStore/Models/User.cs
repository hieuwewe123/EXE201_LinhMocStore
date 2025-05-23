using System;
using System.Collections.Generic;

namespace EXE201_LinhMocStore.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public string? NormalName { get; set; }

    public bool? EmailConfirmed { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Gender { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Username { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public bool? PhoneNumberConfirmed { get; set; }

    public bool? TwoFactorEnabled { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public bool? LockoutEnabled { get; set; }

    public int? AccessFailedCount { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
