
using SHOP_RUNNER.Entities;

namespace SHOP_RUNNER.DTOs.Order_DTO
{
    public class OrderDTO
    {
        public int id { get; set; }
        public int user_id { get; set; }

        public DateTime created_at { get; set; }

        public int grand_total { get; set; }

        public string shipping_address { get; set; }

        public string tel { get; set; }

        public string invoiceId { get; set; }

        // tạo DTO cho status paymentMethod shipping
        public int status_id { get; set; }

        public virtual StatusGetAll Status { get; set; }

        public int payment_method_id { get; set; }

        public virtual MethodPaymentGetAll MethodPayment { get; set; }

        public int shipping_id { get; set; }

        public virtual ShippingGetAll Shipping { get; set; }


    }

    public class OrderDTO2
    {
        public int id { get; set; }
        public int user_id { get; set; }

        public DateTime created_at { get; set; }

        public int grand_total { get; set; }

        public string shipping_address { get; set; }

        public string tel { get; set; }

        public string invoiceId { get; set; }

        // tạo DTO cho status paymentMethod shipping
        public int status_id { get; set; }

        public virtual StatusGetAll Status { get; set; }

        public int payment_method_id { get; set; }

        public virtual MethodPaymentGetAll MethodPayment { get; set; }

        public int shipping_id { get; set; }

        public virtual ShippingGetAll Shipping { get; set; }

        public virtual ShippingGetAll user_entity { get; set; }


    }


    public class DtoDT
    {
        public int product_id { get; set; }

        public int buy_qty { get; set;}

        public int price { get; set; }

        public virtual ShippingGetAll ProductGetName { get; set; }

    }

}
