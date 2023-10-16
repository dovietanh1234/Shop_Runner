using System;
using System.Collections.Generic;

namespace SHOP_RUNNER.Entities;

public partial class Staff
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string? Avatar { get; set; }

    public string Password { get; set; } = null!;

    public string? Role { get; set; }
}