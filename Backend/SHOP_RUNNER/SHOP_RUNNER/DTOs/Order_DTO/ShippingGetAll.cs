namespace SHOP_RUNNER.DTOs.Order_DTO
{
    public class ShippingGetAll
    {
        public string name { get; set; }
    }

    public class CityShipping2
    {
        public int id { get; set; }
        public string name { get; set; }
        public int price_shipping { get; set; }
    }

}
