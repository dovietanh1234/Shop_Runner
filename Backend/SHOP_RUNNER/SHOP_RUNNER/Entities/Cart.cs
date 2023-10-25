using System;
using System.Collections.Generic;

namespace SHOP_RUNNER.Entities;

public partial class Cart
{
    public int? UserId { get; set; }

    public int? ProductId { get; set; }

    public int BuyQty { get; set; }

    public int Id { get; set; }

    public virtual Product? Product { get; set; }

    public virtual User? User { get; set; }
}
