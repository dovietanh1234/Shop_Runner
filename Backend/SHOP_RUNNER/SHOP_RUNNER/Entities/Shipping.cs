using System;
using System.Collections.Generic;

namespace SHOP_RUNNER.Entities;

public partial class Shipping
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
