using System;
using System.Collections.Generic;

namespace SHOP_RUNNER.Entities;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public string? Thumbnail { get; set; }

    public int Qty { get; set; }

    public int CategoryId { get; set; }

    public DateTime CreateDate { get; set; }

    public int UserId { get; set; }

    public int GenderId { get; set; }

    public int BrandId { get; set; }

    public int SizeId { get; set; }

    public int ColorId { get; set; }

    public bool? IsValid { get; set; }

    public virtual Brand Brand { get; set; } = null!;

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Category Category { get; set; } = null!;

    public virtual Color Color { get; set; } = null!;

    public virtual Gender Gender { get; set; } = null!;

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual Size Size { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
