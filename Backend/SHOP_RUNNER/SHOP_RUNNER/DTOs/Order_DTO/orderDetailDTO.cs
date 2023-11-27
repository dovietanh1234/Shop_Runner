using SHOP_RUNNER.Models.name_input;

namespace SHOP_RUNNER.DTOs.Order_DTO
{
    public class orderDetailDTO
    {
        public int? ProductId { get; set; }

        public int OrderId { get; set; }

        public int BuyQty { get; set; }

        public decimal Price { get; set; }

        //public virtual ProductCart Product { get; set; } = null!;
        public string name_p { get; set; }

        public string color_p { get; set; }

        public string size_p { get; set; }
    }
}
