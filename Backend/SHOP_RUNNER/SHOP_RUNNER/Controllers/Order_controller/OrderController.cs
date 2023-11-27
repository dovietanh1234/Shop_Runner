using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PayPalCheckoutSdk.Orders;
using SHOP_RUNNER.DTOs.Order_DTO;
using SHOP_RUNNER.Entities;
using SHOP_RUNNER.Models.name_input;
using SHOP_RUNNER.Models.Order_model;
using SHOP_RUNNER.Services.EmailService;
using SHOP_RUNNER.Services.Order_service;
using System.Net.WebSockets;
using System.Security.Claims;
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
        private HandleOrder_2 _new_order;
        private readonly IEmailService _emailService;

        public OrderController(RunningShopContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _response = null;
            _saboxPayPalClass = new SaboxPaypal_class(_configuration);
            _emailService = emailService;
        }



        [HttpPost, Authorize(Roles = "USER")]
        [EnableRateLimiting("fixedWindow")]
        [Route("payment")]
        public async Task<IActionResult> Order(OrderModel request)
        {
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                if (!identity.IsAuthenticated)
                {
                    return Unauthorized();
                }
                // TA DA CAU HINH LAI ClaimTypes.NameIdentifier -> khi thuc hien cau hinh ACCESS TOKEN co truong "ClaimTypes.NameIdentifier"
                var u_id = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // neu ko co tra ve ngoai le chu ko loi
                int User_1 = Convert.ToInt32(u_id);

                if (request.userId != User_1)
                {
                    return Forbid("you are not permission");
                }


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



            // return invoice_id
            //  string id_invoice = await _new_order.Handle_payment();



            if (request.paymentMethodId == 1)
                {

                    _new_order = new HandleOrder_2(_context, request.userId, request.ShipAddress, request.cityShipId, request.tel, request.paymentMethodId, total_P, invoice_id);
                    int result_order = await _new_order.Handle_payment();
                    if (result_order == 4000)
                    {
                        return BadRequest("fail");
                    }

                    // thanh toán offline:
                    return Ok("Order success! please wait to staff calls for you to validate order... follow status order on website");

                }
                else
                {
                    // Lưu dữ liệu vào session
                    HttpContext.Session.SetString("invoiceId", invoice_id);

                    //CREATE ORDER
                    //_new_order = new HandleOrder_2(_context, request.userId, request.ShipAddress, request.cityShipId, request.tel, request.paymentMethodId, total_P, invoice_id);

                    // SAVE DATA IN DB -> HAM GET PAYPAL TẠO NẾU THANH TOÁN THÀNH CÔNG:
                    var order_success = new HandleOrder()
                    {
                        InvoiceId = invoice_id,
                        UserId = request.userId,
                        ShipAddress = request.ShipAddress,
                        CityShipId = request.cityShipId,
                        Tel = request.tel,
                        PaymentMethodId = request.paymentMethodId,
                        TotalP = total_P,
                    };

                    await _context.HandleOrders.AddAsync(order_success);
                    await _context.SaveChangesAsync();


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

                    Order result = response.Result<Order>();

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
               string invoiceId = HttpContext.Session.GetString("invoiceId");

                // sau khi thanh toán thành công tạo order vào đơn hàng:
                var order_success = await _context.HandleOrders.FirstOrDefaultAsync(o => o.InvoiceId == invoiceId);
                if (order_success != null)
                {
                    _new_order = new HandleOrder_2(
                        _context, 
                        order_success.UserId, 
                        order_success.ShipAddress, 
                        order_success.CityShipId, 
                        order_success.Tel, 
                        order_success.PaymentMethodId, 
                        order_success.TotalP, 
                        order_success.InvoiceId);

                }
                int result_order = await _new_order.Handle_payment();
                if (result_order == 4000)
                {
                    return BadRequest("something wrong");
                }


                HttpContext.Session.Remove("invoiceId");
                _context.HandleOrders.Remove(order_success);
                _context.SaveChanges();

                // thanh toán offline:
                return Ok("Paymen success! your order will deliver early! please follow status order on website");

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



        // USE FOR STAFF:
        // HÀM LẤY CÁC ORDER ĐANG PENDING RA:
        [HttpGet, Authorize(Roles = "Admin, STAFF")]
        [Route("staff/status-order")]
        public IActionResult get_order()
        {
            try
            {
                List<Entities.Order> orders = _context.Orders.Where(o => o.ShipingId == 1)
                                                    .Include(o => o.User)
                                                    .Include( o => o.Status )
                                                    .Include( o => o.PaymentMethod )
                                                    .Include( o => o.Shiping )
                                                    .ToList();

                if (orders.Count == 0)
                {
                    return NotFound();
                }

                //Order_DTO1
                

                // create list DTO:
                List<Order_DTO1> ListDTO = new List<Order_DTO1>();

                // create DTO:
                foreach ( var order in orders )
                {
                    ListDTO.Add(new Order_DTO1()
                    {
                        id = order.Id,
                        user_id = (int)(order.UserId),
                        created_at = order.CreatedAt,
                        grand_total = (int)(order.GrandTotal),
                        shipping_address = order.ShippingAddress,
                        tel  = order.Tel,
                        invoiceId = order.InvoiceId,
                        status_id = order.StatusId,
                        Status = new StatusGetAll()
                        {
                            name = order.Status.Name,
                        },
                        payment_method_id = (int)(order.PaymentMethodId),
                        MethodPayment = new MethodPaymentGetAll()
                        {
                            name = order.PaymentMethod.Name
                        },
                        shipping_id = order.ShipingId,
                        Shipping = new ShippingGetAll()
                        {
                            name = order.Shiping.Name,
                        },
                        User = new user_for_order()
                        {
                            name = order.User.Fullname,
                            email = order.User.Email,
                        }

                    });

                }
                 return Ok(ListDTO);
            }catch(Exception ex) {
                return BadRequest(ex.Message);
            }
        }
      




        // USE FOR STAFF
        // XÁC NHẬN ORDER ( staff's function )
        [HttpPost, Authorize(Roles = "Admin, STAFF")]
        [Route("staff/verify-order")]
        public IActionResult verify_order(int orderId)
        {
            try{
                var order = _context.Orders.Where( o => o.Id == orderId ).Include(o => o.Status)
                                                    .Include(o => o.PaymentMethod)
                                                    .Include(o => o.Shiping)
                                                    .Include(o => o.User).First();

                if (order == null)
                {
                    return BadRequest("dont have any order");
                }

                var invoice_u = _context.Invoices.FirstOrDefault(i => i.InvoiceNo == order.InvoiceId);
                if (invoice_u == null)
                {
                    return BadRequest("not found the invoice");
                }

                if ( order.ShipingId == 2 )
                {
                    return BadRequest("this order verified");
                }


                OrderDTO2 new_o = new OrderDTO2()
                {
                    id = order.Id,
                    user_id = (int)(order.UserId),
                    created_at = order.CreatedAt,
                    grand_total = (int)(order.GrandTotal),
                    shipping_address = order.ShippingAddress,
                    tel = order.Tel,
                    invoiceId = order.InvoiceId,
                    status_id = order.StatusId,
                    Status = new StatusGetAll()
                    {
                        name = order.Status.Name,
                    },
                    payment_method_id = (int)(order.PaymentMethodId),
                    MethodPayment = new MethodPaymentGetAll()
                    {
                        name = order.PaymentMethod.Name
                    },
                    shipping_id = order.ShipingId,
                    Shipping = new ShippingGetAll()
                    {
                        name = order.Shiping.Name,
                    },
                    user_entity = new ShippingGetAll()
                    {
                        name = order.User.Fullname
                    }

                };

                if (order == null)
                {
                    return BadRequest();
                }
                

                // thanh toán offline & online sẽ xử lý khác nhau:
                if ( order.PaymentMethodId == 1 )
                {
                    // thanh toán offline: sửa lại status và shipping in order:
                    invoice_u.Status = "unpaid";
                    order.StatusId = 2;
                    order.ShipingId = 2;

                }
                else
                {
                    invoice_u.Status = "paid";
                    order.ShipingId = 2;
                }

                _context.SaveChanges();

                //SEND MAIL INVOICE FOR CLIENT YOUR ORDER IS DELIVERING:
                var user = _context.Users.FirstOrDefault(u => u.Id == order.UserId);

                List<DtoDT> detail_o = new List<DtoDT>();

                var detail_product = _context.OrderProducts.Where(od => od.OrderId == order.Id).Include(p => p.Product).ToList();

                if (detail_product.Count == 0)
                {
                    return BadRequest("dont have any product in your order");
                }

                foreach ( var p_d in detail_product)
                {
                    detail_o.Add(new DtoDT()
                    {
                        product_id = (int)(p_d.ProductId),
                        buy_qty = p_d.BuyQty,
                        price = (int)(p_d.Price),
                        ProductGetName = new ShippingGetAll()
                        {
                            name = p_d.Product.Name
                        }
                    });
                }

                if (user != null)
                {
                    //RunningShopContext _context
                    _emailService.sentSuccessOrder(user.Email, new_o, detail_o); // nhan vao list<Odetail_product>
                }


                return Ok("order is verify! which is delivering ");
            }catch ( Exception ex )
            {
                return BadRequest( ex.Message );
            }
        }

        [HttpPost, Authorize(Roles = "Admin, STAFF")]
        [Route("staff/search")]
        public IActionResult Search_order(int orderId)
        {
            try
            {
                var order = _context.Orders.Where(o => o.Id == orderId).Include(o => o.Status)
                                                   .Include(o => o.User)
                                                   .Include(o => o.PaymentMethod)
                                                   .Include(o => o.Shiping)
                                                   .Include(o => o.User).First();

                if (order == null)
                {
                    return NotFound("order is not exist");
                }

                return Ok(new Order_DTO1()
                {
                    id = order.Id,
                    user_id = (int)(order.UserId),
                    created_at = order.CreatedAt,
                    grand_total = (int)(order.GrandTotal),
                    shipping_address = order.ShippingAddress,
                    tel = order.Tel,
                    invoiceId = order.InvoiceId,
                    status_id = order.StatusId,
                    Status = new StatusGetAll()
                    {
                        name = order.Status.Name,
                    },
                    payment_method_id = (int)(order.PaymentMethodId),
                    MethodPayment = new MethodPaymentGetAll()
                    {
                        name = order.PaymentMethod.Name
                    },
                    shipping_id = order.ShipingId,
                    Shipping = new ShippingGetAll()
                    {
                        name = order.Shiping.Name,
                    },
                    User = new user_for_order()
                    {
                        name = order.User.Fullname,
                        email = order.User.Email,
                    }

                });

            }
            catch ( Exception ex )
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost, Authorize(Roles = "Admin, STAFF")]
        [Route("staff/detail-invoice")]
        public IActionResult detail_invoice(int oderId)
        {
            try
            {
                var order = _context.Orders.FirstOrDefault(o => o.Id == oderId);

                if (order == null)
                {
                    return NotFound(" not exist order ");
                }

                var invoice = _context.Invoices.FirstOrDefault(i => i.InvoiceNo == order.InvoiceId);

                if ( invoice == null )
                {
                    return NotFound(" not exist invoice ");
                }

                return Ok(invoice);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost, Authorize(Roles = "Admin, STAFF")]
        [Route("staff/detail/order-detail")]
        public IActionResult watch_orderDetail(int orderId)
        {
            try
            {
                var order_detail = _context.OrderProducts.Where( od => od.OrderId == orderId).ToList();

                if ( order_detail.Count == 0 )
                {
                    return NotFound("not found the detail product");
                }

                List<orderDetailDTO> od_dto = new List<orderDetailDTO>();


                foreach ( var od in order_detail)
                {
                    od_dto.Add(new orderDetailDTO()
                    {
                        ProductId = od.ProductId,
                        OrderId = orderId,
                        BuyQty = od.BuyQty,
                        Price = od.Price,
                        name_p = od.NameProduct,
                        size_p = od.SizeProduct,
                        color_p = od.ColorProduct
                    }) ;
                }

                return Ok(order_detail);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        // USE FOR STAFF
        // HUỶ ORDER TỪ PHÍA NHÂN VIÊN:
        [HttpPost, Authorize(Roles = "Admin, STAFF")]
        [Route("staff/cancel-order")]
        public IActionResult cancel_order(int orderId, string reason_cancel)
        {
            try
            {
                var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);



                if (order == null)
                {
                    return BadRequest("not found the order");
                }
                // thanh toán offline & online sẽ xử lý khác nhau:
                // thanh toán offline: sửa lại status và shipping in order:
                var invoice_u = _context.Invoices.FirstOrDefault(i => i.InvoiceNo == order.InvoiceId);
                if (invoice_u == null)
                {
                    return BadRequest("not found the invoice");
                }

                // CẬP NHẬT LẠI HÀNG VỀ KHO:
                var cart_old = _context.OrderProducts.Where( o => o.OrderId == orderId).ToList();

                foreach( var product in cart_old )
                {
                    var new_product = _context.Products.FirstOrDefault(p => p.Id == product.ProductId);

                    if ( new_product != null )
                    {
                        new_product.Qty += product.BuyQty;
                    }
                }


                invoice_u.Status = "cancel";
                    order.StatusId = 5;
                    order.ShipingId = 4;

                _context.SaveChanges();

                // SEND MAIL TO CLIENT THE REASON CANCEL YOUR ORDER:
                var user = _context.Users.FirstOrDefault(u => u.Id == order.UserId);
                if (user != null)
                {
                    _emailService.sentCancelOrder(user.Email, reason_cancel);
                }


                return Ok("order is cacel! your manipulate done! ");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


  


        // USE FOR CLIENT
        // LẤY TRẠNG THÁI ĐƠN HÀNG CHO CLIENT ( lấy các trạng thái đang pending or delivering )
        // phải là chính nó mới lấy  được dữ liệu của nó:
        //TẮT JWT ĐỂ TEST |
        [HttpGet, Authorize(Roles = "USER")]
        [EnableRateLimiting("fixedWindow")]
        [Route("client/status-order-client")]
        public IActionResult status_client(int userId)
        {
            try
            {
                  var identity = HttpContext.User.Identity as ClaimsIdentity;
                 if (!identity.IsAuthenticated)
                  {
                     return Unauthorized();
                }
                // TA DA CAU HINH LAI ClaimTypes.NameIdentifier -> khi thuc hien cau hinh ACCESS TOKEN co truong "ClaimTypes.NameIdentifier"
                   var u_id = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // neu ko co tra ve ngoai le chu ko loi
                   int User_1 = Convert.ToInt32(u_id);


                  if (User_1 != userId)
                  {
                    return Forbid();
                 }

                List<Entities.Order> orders = _context.Orders.Where(o => (o.ShipingId == 1 && o.UserId == userId || o.ShipingId == 2 && o.UserId == userId))
                                                    .Include(o => o.Status)
                                                    .Include(o => o.PaymentMethod)
                                                    .Include(o => o.Shiping)
                                                    .ToList();
                // Nếu ko có return về not found
                if (orders.Count == 0)
                {
                    return NotFound("You have no any orders");
                }

                // create list DTO:
                List<OrderDTO> ListDTO = new List<OrderDTO>();

                // create DTO:
                foreach (var order in orders)
                {
                    ListDTO.Add(new OrderDTO()
                    {
                        id = order.Id,
                        user_id = (int)(order.UserId),
                        created_at = order.CreatedAt,
                        grand_total = (int)(order.GrandTotal),
                        shipping_address = order.ShippingAddress,
                        tel = order.Tel,
                        invoiceId = order.InvoiceId,
                        status_id = order.StatusId,
                        Status = new StatusGetAll()
                        {
                            name = order.Status.Name,
                        },
                        payment_method_id = (int)(order.PaymentMethodId),
                        MethodPayment = new MethodPaymentGetAll()
                        {
                            name = order.PaymentMethod.Name
                        },
                        shipping_id = order.ShipingId,
                        Shipping = new ShippingGetAll()
                        {
                            name = order.Shiping.Name,
                        }

                    });

                }
                return Ok(ListDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        


        // USE FOR CLIENT 
        // XÁC NHẬN CLIENT ĐÃ NHẬN HÀNG THÀNH CÔNG:
        //TẮT JWT ĐỂ TEST |
        [HttpPost, Authorize(Roles = "USER")]
        [Route("client/receive-goods")]
        public IActionResult receive_goods( int orderId, int userId)
        {
            try
            {
                 var identity = HttpContext.User.Identity as ClaimsIdentity;
               if (!identity.IsAuthenticated)
                {
                   return Unauthorized();
               }
                // TA DA CAU HINH LAI ClaimTypes.NameIdentifier -> khi thuc hien cau hinh ACCESS TOKEN co truong "ClaimTypes.NameIdentifier"
                var u_id = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // neu ko co tra ve ngoai le chu ko loi
                int User_1 = Convert.ToInt32(u_id);


               if (User_1 != userId)
                {
                    return Forbid();
                }
              

                var order = _context.Orders.FirstOrDefault(o => ( o.Id == orderId && o.ShipingId == 2) );
                if (order == null)
                {
                    return BadRequest("not found the order");
                }
                // cập nhận nhận hàng thành công:
                order.StatusId = 4;

                _context.SaveChanges();


                return Ok("received goods successfully");
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




        // USE FOR STAFF
        // GET ORDER SUCCESS from user BUT NOT CONFIRM by staff ON SERVER:
        // get by order status 4 & shipping 2 & order id:
        [HttpGet, Authorize(Roles = "Admin, STAFF")]
        [Route("staff/get-success-order")]
        public IActionResult get_success_order()
        {
            try
            {
                List<Entities.Order> orders = _context.Orders.Where(o => (o.ShipingId == 2 && o.StatusId == 4)).Include(o => o.Status).Include(o => o.User)
                                                    .Include(o => o.PaymentMethod)
                                                    .Include(o => o.Shiping).ToList();
                // Nếu ko có return về not found
                if (orders.Count == 0)
                {
                    return NotFound("The user has not received any orders so there is no order");
                }

                List<Order_DTO1> ListDTO = new List<Order_DTO1>();

                // create DTO:
                foreach (var order in orders)
                {
                    ListDTO.Add(new Order_DTO1()
                    {
                        id = order.Id,
                        user_id = (int)(order.UserId),
                        created_at = order.CreatedAt,
                        grand_total = (int)(order.GrandTotal),
                        shipping_address = order.ShippingAddress,
                        tel = order.Tel,
                        invoiceId = order.InvoiceId,
                        status_id = order.StatusId,
                        Status = new StatusGetAll()
                        {
                            name = order.Status.Name,
                        },
                        payment_method_id = (int)(order.PaymentMethodId),
                        MethodPayment = new MethodPaymentGetAll()
                        {
                            name = order.PaymentMethod.Name
                        },
                        shipping_id = order.ShipingId,
                        Shipping = new ShippingGetAll()
                        {
                            name = order.Shiping.Name,
                        },
                        User = new user_for_order()
                        {
                            name = order.User.Fullname,
                            email = order.User.Email,
                        }

                    });

                }
                return Ok(ListDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // USE FOR STAFF
        // GET ORDER SUCCESS from user BUT NOT CONFIRM by staff ON SERVER:
        // get by order status 4 & shipping 2 & order id:
        [HttpGet, Authorize(Roles = "Admin, STAFF")] 
        [Route("staff/get-verified-order")]
        public IActionResult get_verifyOrder()
        {
            try
            {
                List<Entities.Order> orders = _context.Orders.Where(o => (o.ShipingId == 2 && o.StatusId == 2 || o.ShipingId == 2 && o.StatusId == 3)).Include(o => o.Status).Include(o => o.User)
                                                    .Include(o => o.PaymentMethod)
                                                    .Include(o => o.Shiping).ToList();
                // Nếu ko có return về not found
                if (orders.Count == 0)
                {
                    return NotFound("There are no orders in delivering");
                }

                List<Order_DTO1> ListDTO = new List<Order_DTO1>();

                // create DTO:
                foreach (var order in orders)
                {
                    ListDTO.Add(new Order_DTO1()
                    {
                        id = order.Id,
                        user_id = (int)(order.UserId),
                        created_at = order.CreatedAt,
                        grand_total = (int)(order.GrandTotal),
                        shipping_address = order.ShippingAddress,
                        tel = order.Tel,
                        invoiceId = order.InvoiceId,
                        status_id = order.StatusId,
                        Status = new StatusGetAll()
                        {
                            name = order.Status.Name,
                        },
                        payment_method_id = (int)(order.PaymentMethodId),
                        MethodPayment = new MethodPaymentGetAll()
                        {
                            name = order.PaymentMethod.Name
                        },
                        shipping_id = order.ShipingId,
                        Shipping = new ShippingGetAll()
                        {
                            name = order.Shiping.Name,
                        },
                        User = new user_for_order()
                        {
                            name = order.User.Fullname,
                            email = order.User.Email,
                        }

                    });

                }
                return Ok(ListDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




        // USE FOR staff
        // XÁC NHẬN NHẬN HÀNG CHO CLIENT ->staff verify for client
        [HttpPost, Authorize(Roles = "Admin, STAFF")]
        [Route("staff/staff-verify")]
        public IActionResult staff_verify(int orderId)
        {
            try
            {
                var order = _context.Orders.FirstOrDefault(o =>  o.Id == orderId );
                
                if (order == null)
                {
                    return BadRequest("not found the order");
                }
                if (order.ShipingId == 3)
                {
                    return Forbid("order have done!");
                }
                var invoice_u = _context.Invoices.FirstOrDefault(i => i.InvoiceNo == order.InvoiceId);
                if (invoice_u == null)
                {
                    return BadRequest("not found the invoice");
                }

                invoice_u.Status = "successfully";
                // cập nhận nhận hàng thành công:
                order.ShipingId = 3;
                    
                _context.SaveChanges();


                return Ok("verify for client to reveived the order done!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        

        // USE FOR CLIENT
        // CLIENT HUỶ ĐƠN HÀNG KHI ĐANG PENDING:
        //TẮT JWT ĐỂ TEST |
        [HttpPost, Authorize(Roles = "USER")]
        [Route("client/cancel-client")]
        public IActionResult cancel_client(int userId, int orderId ,string reason_cancel )
        {
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
               if (!identity.IsAuthenticated)
               {
                   return Unauthorized();  
                }
                // TA DA CAU HINH LAI ClaimTypes.NameIdentifier -> khi thuc hien cau hinh ACCESS TOKEN co truong "ClaimTypes.NameIdentifier"
                var u_id = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // neu ko co tra ve ngoai le chu ko loi
                int User_1 = Convert.ToInt32(u_id);


               if (User_1 != userId)
               {
                    return Forbid();
               }
                

                var order = _context.Orders.FirstOrDefault(o => (o.UserId == userId && o.Id == orderId ));

                if (order == null)
                {
                    return BadRequest("not found the order");
                }

                if (order.ShipingId == 2)
                {
                    return BadRequest("cannot cancel order when which on the delivering");
                }

                // CẬP NHẬT LẠI HÀNG VỀ KHO:
                var cart_old = _context.OrderProducts.Where(o => o.OrderId == orderId).ToList();

                foreach (var product in cart_old)
                {
                    var new_product = _context.Products.FirstOrDefault(p => p.Id == product.ProductId);

                    if (new_product != null)
                    {
                        new_product.Qty += product.BuyQty;
                    }
                }

                var invoice_u = _context.Invoices.FirstOrDefault(i => i.InvoiceNo == order.InvoiceId);
                if (invoice_u == null)
                {
                    return BadRequest("not found the invoice");
                }

                invoice_u.Status = "cancel";


                //CẬP NHẬT LẠI SỐ LƯỢNG VỀ KHO
                // lấy ra order detail
                // lấy ra trường: product.Quantity += order_product.buyQty



                // thanh toán offline & online sẽ xử lý khác nhau:
                // thanh toán offline: sửa lại status và shipping in order:
                order.StatusId = 5;
                order.ShipingId = 4;

                _context.SaveChanges();

                // SEND MAIL TO CLIENT THE REASON CANCEL YOUR ORDER:
                var user = _context.Users.FirstOrDefault(u => u.Id == order.UserId);
                if (user != null)
                {
                    _emailService.sentCancelOrder(user.Email, reason_cancel);
                }


                return Ok("order is cacel! your manipulate done! ");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        



        // USE FOR CLIENT:
        // LẤY LỊCH SỬ ĐƠN HÀNG CHO CLIENT
        //TẮT JWT ĐỂ TEST |
        [HttpGet, Authorize(Roles = "USER")]
        [EnableRateLimiting("fixedWindow")]
        [Route("client/history-order")]
        public IActionResult history_order(int userId)
        {
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                if (!identity.IsAuthenticated)
                {
                    return Unauthorized();
                }
              //   TA DA CAU HINH LAI ClaimTypes.NameIdentifier -> khi thuc hien cau hinh ACCESS TOKEN co truong "ClaimTypes.NameIdentifier"
                var u_id = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // neu ko co tra ve ngoai le chu ko loi
                int User_1 = Convert.ToInt32(u_id);


                if (User_1 != userId)
                {
                    return Forbid();
                }

                List<Entities.Order> orders = _context.Orders.Where(o => (o.UserId == userId && o.ShipingId == 3 || o.UserId == userId && o.ShipingId == 4))
                                                    .Include(o => o.Status)
                                                    .Include(o => o.PaymentMethod)
                                                    .Include(o => o.Shiping)
                                                    .ToList();

                if (orders.Count == 0)
                {
                    return NotFound("you have no orders");
                }

                // create list DTO:
                List<OrderDTO> ListDTO = new List<OrderDTO>();

                // create DTO:
                foreach (var order in orders)
                {
                    ListDTO.Add(new OrderDTO()
                    {
                        id = order.Id,
                        user_id = (int)(order.UserId),
                        created_at = order.CreatedAt,
                        grand_total = (int)(order.GrandTotal),
                        shipping_address = order.ShippingAddress,
                        tel = order.Tel,
                        invoiceId = order.InvoiceId,
                        status_id = order.StatusId,
                        Status = new StatusGetAll()
                        {
                            name = order.Status.Name,
                        },
                        payment_method_id = (int)(order.PaymentMethodId),
                        MethodPayment = new MethodPaymentGetAll()
                        {
                            name = order.PaymentMethod.Name
                        },
                        shipping_id = order.ShipingId,
                        Shipping = new ShippingGetAll()
                        {
                            name = order.Shiping.Name,
                        }

                    });

                }
                return Ok(ListDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        


  
        // USE FOR STAFF:
        // LẤY LỊCH SỬ ĐƠN HÀNG OR ORDER CHO STAFF OR ADMIN
        [HttpGet, Authorize(Roles = "Admin, STAFF")]
        [Route("staff/history-staff")]
        public IActionResult history_staff()
        {
            try
            {
                // lấy ra các đơn hàng đã thanh toán or đã huỷ:
                List<Entities.Order> orders = _context.Orders.Where(o => ( o.ShipingId == 3 || o.ShipingId == 4))
                                                    .Include(o => o.User)
                                                    .Include(o => o.Status)
                                                    .Include(o => o.PaymentMethod)
                                                    .Include(o => o.Shiping)
                                                    .ToList();

                if (orders.Count == 0)
                {
                    return NotFound("you have no orders");
                }

                List<Order_DTO1> ListDTO = new List<Order_DTO1>();

                // create DTO:
                foreach (var order in orders)
                {
                    ListDTO.Add(new Order_DTO1()
                    {
                        id = order.Id,
                        user_id = (int)(order.UserId),
                        created_at = order.CreatedAt,
                        grand_total = (int)(order.GrandTotal),
                        shipping_address = order.ShippingAddress,
                        tel = order.Tel,
                        invoiceId = order.InvoiceId,
                        status_id = order.StatusId,
                        Status = new StatusGetAll()
                        {
                            name = order.Status.Name,
                        },
                        payment_method_id = (int)(order.PaymentMethodId),
                        MethodPayment = new MethodPaymentGetAll()
                        {
                            name = order.PaymentMethod.Name
                        },
                        shipping_id = order.ShipingId,
                        Shipping = new ShippingGetAll()
                        {
                            name = order.Shiping.Name,
                        },
                        User = new user_for_order()
                        {
                            name = order.User.Fullname,
                            email = order.User.Email,
                        }

                    });

                }
                return Ok(ListDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // get city shiping & method payment:
        [HttpGet]
        [Route("getCityShipping")]
        public IActionResult getCityShipping()
        {

            try
            {
                var result  = _context.CityShippings.ToList();

                if ( result.Count == 0 )
                {
                    return NotFound();
                }

                List<CityShipping2> cts = new List<CityShipping2>();


                foreach ( var c in result )
                {
                    cts.Add(new CityShipping2()
                    {
                        id = c.Id,
                        name = c.Name,
                        price_shipping = (int)(c.PriceShipping)
                    });
                }

                return Ok(cts);


            }catch(Exception ex)
            {
                return BadRequest();
            }

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
            if (products_cart.Count == 0)
            {
                throw new Exception("fail your cart is empty");
            }

            int Total_price = 0;
            List<Item> cart_new = new List<Item>();

            foreach (var c_p in products_cart)
            {
                // lấy ra giá của sản phẩm
                var product = await _context.Products.FirstOrDefaultAsync( p => p.Id == c_p.ProductId);
                // CHECK NẾU SẢN PHẨM IsActive == FALSE

                if (product.IsValid == true) { 
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
                }
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
