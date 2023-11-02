using System.ComponentModel.DataAnnotations;

namespace SHOP_RUNNER.Models.Order_model
{
    public class OrderModel
    {
        [Required]
        public int userId { get; set; }

        public string consignee_name { get; set; } = "not found";

        public string ShipAddress { get; set; } = "Homeless";
        [Required]
        public int cityShipId { get; set; }
        [Required]
        public string tel { get; set; }
        [Required]
        public int paymentMethodId { get; set; }
    }
}
