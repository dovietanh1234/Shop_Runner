using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayPalCheckoutSdk.Orders;
using SHOP_RUNNER.Entities;
using SHOP_RUNNER.Models.Order_model;
using SHOP_RUNNER.Services.Order_service;
using System.Net.WebSockets;
using Order = PayPalCheckoutSdk.Orders.Order;

namespace SHOP_RUNNER.Controllers.Order_controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly RunningShopContext _context;
        private readonly IConfiguration _configuration;
        private readonly SaboxPaypal_class _saboxPayPalClass;
        private readonly HttpResponse _response;
        private HandleOrder _new_order;

        public OrderController(RunningShopContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _response = null;
            _saboxPayPalClass = new SaboxPaypal_class(_configuration);
        }

        [HttpPost]
        [Route("payment")]
        public async Task<IActionResult> Order(OrderModel request)
        {
            try
            {
 //userId    ShipAddress ( truy vấn để default )    cityShipId      tel      paymentMethodId   consignee_name( tên người nhận hàng ) ( để default )
            
                // thanh toán online paypal:

                // tạo một hàm trả về List sản phẩm:
                HandlePrice Data1 = await Products_user(request.userId);
                // Data.List_item -> lấy ra một list dữ liệu
                // Data.total_price -> lấy ra tổng giá sản phảm

                //  int totalPrice = ((dynamic)Data1).total_price;
                // List<Item> Data_list = ((dynamic)Data1).List_item;
                // ta có thể ép kiểu dynamic cho object or tạo một model trả về dữ liệu cho yên tâm:


                // lấy các parameter cho order:
                var city_ship = await _context.CityShippings.FirstOrDefaultAsync(cs => cs.Id == request.cityShipId);

                int total_P = Data1.total_price; // đưa total price vào cho hàm order
                List<Item> Data_list = Data1.item;

            //Lấy random string và chuyền vào HandleOrder:
            string invoice_id = await RandomString();

                //CREATE ORDER
                _new_order = new HandleOrder(_context, request.userId, request.ShipAddress, request.cityShipId, request.tel, request.paymentMethodId, total_P, invoice_id);
            // return invoice_id
            //  string id_invoice = await _new_order.Handle_payment();




          /*  int result_order = await _new_order.Handle_payment();
            if (result_order == 4000)
            {
                return BadRequest("fail");
            }

            */




            if (request.paymentMethodId == 1)
                {
                // thanh toán offline:
                    return Ok("Order success! please wait to staff calls for you to validate order... follow status order on website");

                }
                else
                {
                int value_price = total_P + (int)(total_P * 0.01) + (int)(city_ship.PriceShipping);

                    var order = new OrderRequest()
                    {
                        CheckoutPaymentIntent = "CAPTURE",
                        PurchaseUnits = new List<PurchaseUnitRequest>()
                        {
                            new PurchaseUnitRequest()
                            {
                                AmountWithBreakdown = new AmountWithBreakdown()
                                {
                                    CurrencyCode = "USD",
                                    Value = value_price.ToString(),
                                    AmountBreakdown = new AmountBreakdown()
                                    {
                                        ItemTotal = new Money()
                                        {
                                            CurrencyCode = "USD",
                                            Value = total_P.ToString()
                                        },
                                        TaxTotal = new Money()
                                        {
                                            CurrencyCode = "USD",
                                            Value = ((int)(total_P * 0.01)).ToString()
                                        },
                                        Shipping = new Money()
                                        {
                                            CurrencyCode = "USD",
                                            Value = ((int)(city_ship.PriceShipping)).ToString()
                                        }
                                    }
                                },
                                Items = Data_list,
                                Payee = new Payee()
                                {
                                    Email = "conbonha2k@gmail.com"
                                },
                                ShippingDetail = new ShippingDetail()
                                {
                                    Name = new Name()
                                    {
                                        FullName = request.consignee_name
                                    },
                                    AddressPortable = new AddressPortable()
                                    {
                                        AddressLine1 = request.ShipAddress,
                                        AddressLine2 = request.ShipAddress,
                                        AdminArea1 = request.ShipAddress,
                                        AdminArea2 = request.ShipAddress,
                                        PostalCode = "000084",
                                        CountryCode = "VN"
                                    }
                                },
                                InvoiceId = invoice_id
                            }
                        },
                        ApplicationContext = new ApplicationContext()
                        {
                            ReturnUrl = "https://localhost:7208/api/Order/success", 
                            CancelUrl = "https://localhost:7208/api/Order/cancel" 
                        }
                    };



                    // call Api paypal with your client and get a response for your call:
                    var request_1 = new OrdersCreateRequest();
                    request_1.Prefer("return=representation");
                    request_1.RequestBody(order);

                    var response = await _saboxPayPalClass.Client().Execute(request_1);

                    if (response.StatusCode != System.Net.HttpStatusCode.Created)
                    {
                        return BadRequest("fail to payment");
                    }

                    PayPalCheckoutSdk.Orders.Order result = response.Result<PayPalCheckoutSdk.Orders.Order>();

                    return Ok(new
                    {
                        link1 = result.Links[0].Href,
                        link2 = result.Links[1].Href,
                        link3 = result.Links[2].Href,
                    });



                }
            }catch(Exception ex) { 
            return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("success")]
        public async Task<IActionResult> payment_success(string PayerID, string token)
        {


            try
            {
                var request = new OrdersCaptureRequest(token);

                request.RequestBody(new OrderActionRequest());

                var response = await _saboxPayPalClass.Client().Execute(request);



                //   var order = response.Result<PayPalCheckoutSdk.Orders.Order>();


                //var invoiceId = order.PurchaseUnits[0].InvoiceId;

              /*  var order = response.Result<Order>();
                var order1 = order.PurchaseUnits[0].InvoiceId;
              */

                return Ok("payment success");

            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        

        [HttpGet]
        [Route("cancel")]
        public IActionResult payment_fail()
        {
            return BadRequest("payment fail");
        }











        private static async Task<string> RandomString(int length = 6)
        {
            Random random = new Random();
            string character = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            // task chỉ sử dụng trong trường hợp liên quan đến việc tính toán một việc nhất định ...
            // hoặc là những việc liên quan đến độ trễ của DB nhưng ko nhớ ví dụ 
            Task<string> task = Task.Run(() => new string(Enumerable.Repeat(character, length).Select(s => s[random.Next(s.Length)]).ToArray()));

            string result = await task;

            return result;
        }



        private async Task<HandlePrice> Products_user(int userId)
        {
            // lấy ra các sản phẩm trong cart:
            var products_cart = await _context.Carts.Where(c => c.UserId == userId).ToListAsync();

            int Total_price = 0;
            List<Item> cart_new = new List<Item>();

            foreach (var c_p in products_cart)
            {
                // lấy ra giá của sản phẩm
                var product = await _context.Products.FirstOrDefaultAsync( p => p.Id == c_p.ProductId);
                // CHECK NẾU SẢN PHẨM IsActive == FALSE

                // Add sản phẩm vào cart 
                cart_new.Add(new Item()
                {
                    Name = product.Name,
                    UnitAmount = new Money()
                    {
                        CurrencyCode = "USD",
                        Value = product.Price.ToString()
                    },
                    Quantity = c_p.BuyQty.ToString()
                });

                // lấy ra tổng tiền:
                Total_price += (int)(product.Price) * c_p.BuyQty;
            }

            return new HandlePrice()
            {
                item = cart_new,
                total_price = Total_price,
            };
        }






    }
}
