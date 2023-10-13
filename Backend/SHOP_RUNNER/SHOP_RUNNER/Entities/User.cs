using System;
using System.Collections.Generic;

namespace SHOP_RUNNER.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Fullname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Avatar { get; set; }

    public string? Tel { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string Role { get; set; } = null!;

    public byte[] PasswordSalt { get; set; } = null!;

    public byte[] PasswordHash { get; set; } = null!;

    public string VerificationToken { get; set; } = null!;

    public DateTime? VerifiedAt { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? ResetTokenExpires { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
