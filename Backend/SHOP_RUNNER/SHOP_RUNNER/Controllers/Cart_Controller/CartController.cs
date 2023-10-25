using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SHOP_RUNNER.Services.Cart_service;

namespace SHOP_RUNNER.Controllers.Cart_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICart_m _ICart;

        public CartController(ICart_m icart)
        {
            _ICart = icart;
        }

        [HttpPost]
        [Route("add-to-cart")]
        public IActionResult add_to_cart(int userId, int product_id)
        {
            try {
                int result = _ICart.createCart(userId, product_id);

                return Ok(result == 0002 ? "add to cart success" : "run out of the product in cart");
            
            }catch(Exception ex) {
                return BadRequest(ex.Message);
            
            }

        }

        // đoạn code này -> chỉ sử dụng trong cart -> vì nó check điều kiện phải có sản phẩm trong cart thì mới alter kết quả nhé!
        [HttpPost]
        [Route("alter_quantity")]
        public IActionResult updateCart(int userId, int product_id, int? plus, int? minus, int? quantity)
        {
            try
            {

                int result = _ICart.updateCart( userId, product_id, plus, minus, quantity );
                if ( result == 4001 )
                {
                    return NotFound("run out of stock");
                }
                if (result == 4003)
                {
                    return NotFound("run out of product in cart");
                }
                if (result == 0001)
                {
                    return Ok("not have any value pass in...");
                }

                return Ok("success manipulation");


            }catch(Exception ex) { 
                return BadRequest( ex.Message );
            }
        }

        [HttpDelete]
        [Route("delete-product-cart")]
        public IActionResult delete_product(int userId, int product_id)
        {
            try
            {
                int result = _ICart.delete_product(userId, product_id);

                return Ok( result == 4000? "some error occur please try later":"success manipulation" );


            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("")]
        public IActionResult delete_cart(int userId)
        {
            try
            {

                int result = _ICart.delete_cart(userId);
                return Ok(result == 4003 ? "run out of product in cart" : "success manipulation");

            }catch(Exception ex) {
                return BadRequest(ex.Message);
            }
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