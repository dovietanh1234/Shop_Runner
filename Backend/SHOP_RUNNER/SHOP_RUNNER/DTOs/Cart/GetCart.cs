namespace SHOP_RUNNER.DTOs.Cart
{
    public class GetCart
    {
        public int qty { get; set; }

        public int productId { get; set; }

        public virtual ProductCart Product { get; set; } = null!;

    }
}
