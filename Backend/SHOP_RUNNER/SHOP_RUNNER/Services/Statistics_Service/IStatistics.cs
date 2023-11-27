using SHOP_RUNNER.Models.Statistics;

namespace SHOP_RUNNER.Services.Statistics_Service
{
    public interface IStatistics
    {
        // THỐNG KÊ THEO THÁNG

        // tháng trước kiếm đc bao nhiêu
        decimal price_last_month();
        // tháng trước nữa kiếm được bao nhiêu
        decimal price_last_month2();


        // tổng tiền tháng này
        decimal price_month();
        int ordersMonth();
        int orderSuccess();
        List<product_DTO_NAME> top3Soldest();
        List<product_DTO_NAME> product_sold();
        List<Entities.OrderProduct> product_detail(int productId);



        // THỐNG KÊ THEO TUẦN
        // tổng số tiền đã thanh toán trong 1 tuần
        decimal pice_week();

        int ordersWeek();

        int orderSuccessWeek();

        // tổng số sản phẩm bán ra trong tuần
        List<top3soldest> sold_week();


        // tổng số tiền đã BÁN THEO NGÀY:
        decimal pice_day();

        int ordersDay();

        int orderSuccessDay();

        // tổng số sản phẩm bán ra trong tuần
        List<top3soldest> sold_day();


    }
}
