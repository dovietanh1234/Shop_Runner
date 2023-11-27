namespace SHOP_RUNNER.Models.Statistics
{
    public class total_month
    {
        public decimal TotalAmount { get; set; }
    }

    public class orders_a_month
    {
        public int total_order { get; set; }
    }

    public class top3soldest
    {
        public int product_id { get; set; }
        public int SELLING { get; set; }
    }

    public class Caculate_time
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class product_DTO_NAME
    {
        public int product_id { get; set; }
        public string name { get; set; }
        public string price { get; set; }

        public string thumbnail { get; set; }

        public int SELLING { get; set; }

    }

}
