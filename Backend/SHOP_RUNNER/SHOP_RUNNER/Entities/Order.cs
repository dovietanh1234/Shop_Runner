using System;
using System.Collections.Generic;

namespace SHOP_RUNNER.Entities;

public partial class Order
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public decimal GrandTotal { get; set; }

    public string? ShippingAddress { get; set; }

    public string? Tel { get; set; }

    public string InvoiceId { get; set; } = null!;

    public int StatusId { get; set; }

    public int? IdCityShip { get; set; }

    public int? PaymentMethodId { get; set; }

    public int ShipingId { get; set; }

    public virtual CityShipping? IdCityShipNavigation { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual MethodPayment? PaymentMethod { get; set; }

    public virtual Shipping Shiping { get; set; } = null!;

    public virtual Status Status { get; set; } = null!;

    public virtual User? User { get; set; }
}
