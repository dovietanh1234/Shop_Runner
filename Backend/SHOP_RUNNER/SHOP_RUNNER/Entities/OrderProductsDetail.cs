using System;
using System.Collections.Generic;

namespace SHOP_RUNNER.Entities;

public partial class OrderProductsDetail
{
    public int? ProductId { get; set; }

    public int OrderId { get; set; }

    public int BuyQty { get; set; }

    public decimal Price { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product? Product { get; set; }
}
