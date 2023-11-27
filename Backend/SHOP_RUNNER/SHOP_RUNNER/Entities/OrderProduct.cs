using System;
using System.Collections.Generic;

namespace SHOP_RUNNER.Entities;

public partial class OrderProduct
{
    public int? ProductId { get; set; }

    public int OrderId { get; set; }

    public int BuyQty { get; set; }

    public decimal Price { get; set; }

    public int Id { get; set; }

    public string? NameProduct { get; set; }

    public string? ColorProduct { get; set; }

    public string? SizeProduct { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product? Product { get; set; }
}
