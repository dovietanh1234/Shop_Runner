using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using SHOP_RUNNER.Services.Cart_service;
using System.Security.Claims;

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

        //  [HttpGet(Name = "GetWeatherForecast"), Authorize(Roles = "Admin")] // Authorize(Roles = "Admin,Staff")

        // TẮT CHỨC NĂNG TOKEN:
        // 
        [HttpPost, Authorize(Roles = "USER")]
        [EnableRateLimiting("fixedWindow")]
        [Route("add-to-cart")]
        public IActionResult add_to_cart(int userId, int product_id)
        {
            try {
                
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                if (!identity.IsAuthenticated)
                {
                    return Unauthorized();
                }
                // TA DA CAU HINH LAI ClaimTypes.NameIdentifier -> khi thuc hien cau hinh ACCESS TOKEN co truong "ClaimTypes.NameIdentifier"
                var u_id = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // neu ko co tra ve ngoai le chu ko loi
                int User_1 = Convert.ToInt32(u_id);

                if (userId != User_1)
                {
                    return Forbid("you are not permission");
                }
                

                int result = _ICart.createCart(userId, product_id);

                return Ok(result == 0002 ? "add to cart success" : "run out of the product in cart");
            
            }catch(Exception ex) {
                return BadRequest(ex.Message);
            
            }

        }

        // đoạn code này -> chỉ sử dụng trong cart -> vì nó check điều kiện phải có sản phẩm trong cart thì mới alter kết quả nhé!

        // TẮT CHỨC NĂNG TOKEN:
        //   | 
        [HttpPost, Authorize(Roles = "USER")]
        [Route("alter_quantity")]
        public IActionResult updateCart(int userId, int product_id, int? plus, int? minus, int? quantity)
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

                if (userId != User_1)
                {
                    return Forbid("you are not permission");
                }
                
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


        // TẮT CHỨC NĂNG TOKEN:
        // 
        [HttpDelete, Authorize(Roles = "USER")]
        [Route("delete-product-cart")]
        public IActionResult delete_product(int userId, int product_id)
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

                if (userId != User_1)
                {
                    return Forbid("you are not permission");
                }
                
                int result = _ICart.delete_product(userId, product_id);

                return Ok( result == 4000? "some error occur please try later":"success manipulation" );


            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // TẮC CHỨC NĂNG TOKEN:
        // 
        [HttpDelete, Authorize(Roles = "USER")]
        [Route("delete-cart")]
        public IActionResult delete_cart(int userId)
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

                if (userId != User_1)
                {
                    return Forbid("you are not permission");
                }


                int result = _ICart.delete_cart(userId);
                return Ok(result == 4003 ? "run out of product in cart" : "success manipulation");

            }catch(Exception ex) {
                return BadRequest(ex.Message);
            }
        }



        // GET ALL CART:
        // tắt chức năng token: | 
        [HttpGet, Authorize(Roles = "USER")]
        [EnableRateLimiting("fixedWindow")]
        [Route("get-cart")]
        public IActionResult get_cart(int userId)
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

                if (userId != User_1)
                {
                    return Forbid("you are not permission");
                }
                

                // GET ALL CART:
              
                return Ok(_ICart.get_cart(userId));
            }
            catch (Exception ex)
            {
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