using System;
using System.Collections.Generic;

namespace SHOP_RUNNER.Entities;

public partial class HandleOrder
{
    public string InvoiceId { get; set; } = null!;

    public int UserId { get; set; }

    public string ShipAddress { get; set; } = null!;

    public int CityShipId { get; set; }

    public string Tel { get; set; } = null!;

    public int PaymentMethodId { get; set; }

    public int TotalP { get; set; }
}
