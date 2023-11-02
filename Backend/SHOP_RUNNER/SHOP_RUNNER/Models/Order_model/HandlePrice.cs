using PayPalCheckoutSdk.Orders;

namespace SHOP_RUNNER.Models.Order_model
{
    public class HandlePrice
    {
        public List<Item> item { get; set; }

        public int total_price { get; set; }
    }
}
