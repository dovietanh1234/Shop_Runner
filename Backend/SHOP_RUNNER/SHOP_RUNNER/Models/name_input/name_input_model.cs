namespace SHOP_RUNNER.Models.name_input
{
    public class name_input_model
    {
        public string name { get; set; }
    }

    public class order_detail_dto
    {
        public int? ProductId { get; set; }

        public int OrderId { get; set; }

        public int BuyQty { get; set; }

        public decimal Price { get; set; }

        //public virtual ProductCart Product { get; set; } = null!;

        public virtual name_input_model Product { get; set; } = null;

    }

}
