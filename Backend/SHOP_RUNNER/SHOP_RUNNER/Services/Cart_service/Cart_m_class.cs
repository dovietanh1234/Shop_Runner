using Microsoft.EntityFrameworkCore;
using SHOP_RUNNER.DTOs.Cart;
using SHOP_RUNNER.Entities;
using System.Xml.Linq;

namespace SHOP_RUNNER.Services.Cart_service
{
    public class Cart_m_class : ICart_m
    {
        private readonly RunningShopContext _context;
        public Cart_m_class( RunningShopContext context )
        {
            _context = context;
        }

        // ADD TO CART
        public int createCart(int userId, int product_id)
        {
            var product_subtract = _context.Products.FirstOrDefault( p => p.Id == product_id );

            var Cart_item = _context.Carts.FirstOrDefault( c => (c.UserId == userId && c.ProductId == product_id));


            if( product_subtract != null )
            {
                if (product_subtract.IsValid != true)
                {
                    return 4003;
                }

                if (product_subtract.Qty <= 0)
                            {
                                return 4001;
                            }
                            else
                            {
                                product_subtract.Qty -= 1;
                            }

                            // Nếu cart_item đang có dữ liệu
                            if (Cart_item != null )
                            {
                                Cart_item.BuyQty += 1;
                            }
                            else
                            {
                                var new_cart = new SHOP_RUNNER.Entities.Cart()
                                {
                                    UserId = userId,
                                    ProductId = product_id,
                                    BuyQty = 1
                                };
                                _context.Carts.Add( new_cart );
                            }
            }
            _context.SaveChanges();
            return 0002;


        }
    
        // ĐẾN PHẦN ĐI VÀO CART ( TĂNG OR GIẢM -> ĐƯA GIÁ TRỊ QTY VÀO SẢN PHẨM )
        public int updateCart(int userId, int product_id, int? plus, int? minus, int? quantity)
        {
            // trong cart là những sản phẩm đã có sãn rồi tìm và lấy ra:

            var product_inCart = _context.Carts.FirstOrDefault( c => (c.UserId == userId && c.ProductId == product_id) );

            var product_subtract = _context.Products.FirstOrDefault( p => p.Id==product_id );

            // chia product_subtract -> 2 cases: trường hợp 1 ( - qty product + qty cart )  | trường hợp 2 ( + qty product - qty cart )

            // ? vậy thì cái quantity -> biết lúc nào là client nó nhập vào số trừ || nó nhập vào số + lên -> 

            // ta phải lấy data cũ so sánh với quantity mới để biết nó lớn hơn hay nó nhỏ hơn
            // nếu nó lớn hơn -> thì phải biết số khoảng cách nó đã tăng vào là bao nhiêu ví dụ ( cũ là 5 sp -> nó nhập vào 7 ) thì ta phải biết nó đã nhập vào tăng 2 sản phẩm và trừ đi trong kho
            // nếu

            // nếu nó nhập nhỏ hơn qty cũ -> thì ta phải biết khoảng cách nó nhập vào giá trị cũ là bnhiu ví dụ ( cũ là 5 nó nhập vào là 3 ) -> thì khoảng cách sẽ là 3
            // update mới cho cart.qty = 3 và cộng thêm 2 trong product.qty

            // nếu nó update qutity == 0 && và minus về => 1 tức là nó đang muốn loại sản phẩm khỏi cart ( Mình sẽ để cho nó cập nhật về == 0 ) Nó có thể tự xoá sản phẩm
            // thanh toán -> kiểm tra sản phẩm nào trong cart có quantity == 0 -> thì loại!

            if (product_subtract.IsValid != true)
            {
                return 4003;
            }

            if ( product_subtract != null && product_inCart != null)
            {
                            #region Press plus button
                                        if (plus.HasValue)
                                        {
                                            if (product_subtract.Qty >= 1)
                                            {
                                                product_inCart.BuyQty += 1;
                                                product_subtract.Qty -= 1;
                                                _context.SaveChanges();
                                                return 0002;
                                            }
                                            return 4001;
                                        }
                              #endregion

                            #region Press subtract button
                            if (minus.HasValue)
                            {
                                if (product_inCart.BuyQty >= 1)
                                {
                                    product_inCart.BuyQty -= 1;
                                    product_subtract.Qty += 1;
                                    _context.SaveChanges();
                                    return 0002;
                                }
                                return 4003;
                            }
                            #endregion

                            #region Pass some value in input
                            if ( quantity.HasValue )
                            {
                                // xử lý quan trọng nhất:
                                if (product_inCart.BuyQty < quantity)
                                {
                                    // người dùng nhập vào số lớn hơn
                                    // trước khi update kiểm tra xem số lượng trong kho có đủ ko?
                    
                                    int surplus = (int)(quantity) - product_inCart.BuyQty;
                                    // nếu như trong kho còn sản phẩm 

                                    //so luong trong kho:
                                    int qty_stock = product_subtract.Qty - surplus;
                                    if ( qty_stock >= 0 )
                                    {
                                        // ta sẽ update quantity vào trường BuyQty
                                        product_inCart.BuyQty = (int)(quantity);
                                        // lấy khoảng cách và trừ đi số lượng trong kho product
                                        product_subtract.Qty -= surplus;
                                        _context.SaveChanges();
                                        return 0002;
                                    }
                                    return 4001;

                                }
                                else
                                {
                                    // người dùng nhập vào số nhỏ hơn:
                                    int surplus = product_inCart.BuyQty - (int)(quantity);
                                    // trừ qty trong cart -> nếu nhỏ or = 0 thì xử lý ...
                                    product_inCart.BuyQty = (int)(quantity);

                                    // + qty cho product trong kho:
                                    product_subtract.Qty += surplus;
                                    // nếu quantity == 0 thì ...
                                    _context.SaveChanges();
                                    if (quantity == 0)
                                    {
                                        return 4003;
                                    }
                                    return 0002;
                                }
                            }
                            #endregion
            }

         

            // nếu ko truyền dữ liệu gì?
            return 0001;
        }

        // XOÁ 1 SẢN PHẨM KHỎI CART THEO UserId
        public int delete_product(int userId, int product_id)
        {
            // tìm product trong cart rồi delete nó:
            var Cart_item = _context.Carts.FirstOrDefault(c => (c.UserId == userId && c.ProductId == product_id));

            var product_subtract = _context.Products.FirstOrDefault(p => p.Id == product_id);

            if (Cart_item == null || product_subtract == null)
            {
                return 4000;
            }

            product_subtract.Qty += Cart_item.BuyQty;

            _context.Carts.Remove(Cart_item);
            _context.SaveChanges();
            return 0002;
        }

        // XOÁ ALL SẢN PHẨM KHỎI CART THEO UserId
        public int delete_cart(int userId)
        {
            var Cart_item = _context.Carts.Where(c => c.UserId == userId ).ToList();

            if(Cart_item.Count > 0)
            {
                foreach ( var cart_user in Cart_item)
                {
                    // tìm sản phẩm và update product cho kho...
                    var product = _context.Products.FirstOrDefault(p => p.Id == cart_user.ProductId);
                    if (product != null)
                    {
                        product.Qty += cart_user.BuyQty;
                    }
                }

                _context.Carts.RemoveRange(Cart_item);
                _context.SaveChanges();
            }
            else
            {
                return 4003;
            }
            return 0002;
        }

        public List<GetCart> get_cart(int userId)
        {
            var Cart_item = _context.Carts.Where( c => c.UserId == userId ).Include( c => c.Product ).ToList();
            if(Cart_item.Count <= 0)
            {
                return null;
            }

            List<GetCart> Dto = new List<GetCart>();

            foreach ( var element in Cart_item )
            {

                Dto.Add(new GetCart()
                {
                    qty = element.BuyQty,
                    productId = (int)(element.ProductId),
                    Product = new ProductCart()
                    {
                        Name = element.Product.Name,
                        Price = element.Product.Price,
                        Thumbnail = element.Product.Thumbnail
                    }

                });

            }


            return Dto;


        }

    }
}


/*
 xử lý mã lỗi: 
0001: không có giá trị chuyền vào
4000: lỗi chung
4001: là hết sản phẩm trong kho
0002: là thao tác hoàn thành công!
4003: số lượng phần tử trong cart đã hết 

Xử lý tắt sản phẩm ko xoá được sản phẩm!
 */