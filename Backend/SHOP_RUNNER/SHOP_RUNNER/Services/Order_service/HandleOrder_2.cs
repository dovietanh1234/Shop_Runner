using Microsoft.EntityFrameworkCore;
using SHOP_RUNNER.Entities;

namespace SHOP_RUNNER.Services.Order_service
{
    public class HandleOrder_2
    {
        private readonly RunningShopContext _context;

        private int _userId;
        private string _shipAddress;
        private int _cityShipId;
        private string _tel;
        private int _paymentMethodId;
        private int _price_in_cart;
        private string _invoice_id;

        public HandleOrder_2(RunningShopContext context, int userId, string ShipAddress, int cityShipId, string tel, int paymentMethodId, int price_in_cart, string invoice_id)
        {
            _context = context;
            _userId = userId;
            _shipAddress = ShipAddress;
            _cityShipId = cityShipId;
            _tel = tel;
            _paymentMethodId = paymentMethodId;
            _price_in_cart = price_in_cart;
            _invoice_id = invoice_id;
        }

        public async Task<int> Handle_payment()
        {
            //string invoice_id = await RandomString();
            
            // lấy ra kiểu thanh toán của người dùng:
            var payment_method =  _context.MethodPayments.FirstOrDefault( p => p.Id == _paymentMethodId );

            if ( payment_method == null ) {
                return 4000;
            }

            bool cart_check = await _context.Carts.AnyAsync(c => c.UserId == _userId);

            if (cart_check == false)
            {
                return 4000;
            }

            if ( _paymentMethodId == 1 )
            {
                // thanh toán offline

                //1.create invoice:
                await CreateInvoice(_invoice_id, _price_in_cart, 2);

                //2. create order:
                await CreateTblOrder(_price_in_cart, _invoice_id, 1, 1);

                //3. create order detail:
               await CreateOrderDetail();

            }
            else
            {
                // thanh toán online

                //1.create invoice:
                await CreateInvoice(_invoice_id, _price_in_cart, 3);

                //2. create order:
                await CreateTblOrder(_price_in_cart, _invoice_id, 3, 1);

                //3. create order detail:
                await CreateOrderDetail();

            }
            return 2000;
        }

        // lỗi 4000 và lỗi 2000






        private async Task CreateInvoice(string InvoiceNo, int total_price, int status_id)
        {
            var CityShip = await _context.CityShippings.FirstOrDefaultAsync( c => c.Id == _cityShipId );
            var method_p = await _context.MethodPayments.FirstOrDefaultAsync( p => p.Id == _paymentMethodId );
            var status_p = await _context.Statuses.FirstOrDefaultAsync(s => s.Id  == status_id);

            var Invoice = new Invoice()
            {
                InvoiceNo = InvoiceNo,
                CreatedAt = DateTime.Now,
                TotalMoney = price_invoice( total_price, (int)(CityShip.PriceShipping)),
                PaymentMethod = method_p.Name,
                City = CityShip.Name,
                Status = status_p.Name
            };

            await _context.Invoices.AddAsync( Invoice );
            await _context.SaveChangesAsync();
            return;
        }


        private async Task CreateOrderDetail()
        {
            var carts = await _context.Carts.Where( c => c.UserId == _userId ).ToListAsync() ;

            var order_id = await _context.Orders.Where(o => o.UserId == _userId).OrderByDescending(o => o.Id).FirstOrDefaultAsync();

            if (order_id == null)
            {
                return;
            }

            foreach( var c in carts )
            {
                var product = await _context.Products.Include(p => p.Size).Include(p => p.Color).FirstOrDefaultAsync(p => p.Id == c.ProductId);
                //  var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == c.ProductId);


                // XỬ LÝ NẾU SẢN PHẨM isActive == false;
                if (product.IsValid == true)
                {
                    await _context.OrderProducts.AddAsync(new OrderProduct()
                    {
                        ProductId = c.ProductId,
                        OrderId = order_id.Id,
                        BuyQty = c.BuyQty,
                        Price = product.Price,
                        NameProduct = product.Name,
                        ColorProduct = product.Color.Name,
                        SizeProduct = product.Size.Name,
                    });
                }
                

             

            }
                // đoạn nay sai logic phải khi người dùng thay đổi trạng thái thì họ có thể xoá cart cũ đi 
                _context.Carts.RemoveRange(carts);
                await _context.SaveChangesAsync();


        }


        private int price_invoice(int total, int price_ship)
        {
            return total += (int)(total * 0.01) + price_ship;
        }

        private async Task CreateTblOrder(int price_cart, string InvoiceId, int status_id, int ship_id)
        {
            var Order = new Order()
            {
                UserId = _userId,
                CreatedAt = DateTime.Now,
                GrandTotal = price_cart,
                ShippingAddress = _shipAddress,
                Tel = _tel,
                InvoiceId = InvoiceId,
                StatusId = status_id,
                ShipingId = ship_id,
                IdCityShip = _cityShipId,
                PaymentMethodId = _paymentMethodId
            };

            await _context.Orders.AddAsync( Order );
            await _context.SaveChangesAsync();
        }

        private static async Task<string> RandomString(int length = 6)
        {
            Random random = new Random();
            string character = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            // task chỉ sử dụng trong trường hợp liên quan đến việc tính toán một việc nhất định ...
            // hoặc là những việc liên quan đến độ trễ của DB nhưng ko nhớ ví dụ 
            Task<string> task = Task.Run(() => new string( Enumerable.Repeat(character, length).Select( s => s[random.Next(s.Length)]).ToArray() ) );

            string result = await task;

            return result;
        }

    }
}
