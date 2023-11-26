using SHOP_RUNNER.Common;
using System.ComponentModel.DataAnnotations;

namespace SHOP_RUNNER.Models.Order_model
{
    public class OrderModel
    {
        private string _consignee_name;
        private string _ShipAddress;
        private string _tel;

        [Required]
        public int userId { get; set; }

        public string consignee_name { get { return _consignee_name; } set { _consignee_name = filter_characters.cleanInput(value); } }

        public string ShipAddress { get { return _ShipAddress; } set { _ShipAddress = filter_characters.cleanInput(value); } }
        [Required]
        public int cityShipId { get; set; }
        [Required]
        public string tel { get; set; }
        [Required]
        public int paymentMethodId { get; set; }
    }



}
