using System;
using System.Collections.Generic;

namespace SHOP_RUNNER.Entities;

public partial class Invoice
{
    public string InvoiceNo { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public decimal TotalMoney { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual Order? Order { get; set; }
}
