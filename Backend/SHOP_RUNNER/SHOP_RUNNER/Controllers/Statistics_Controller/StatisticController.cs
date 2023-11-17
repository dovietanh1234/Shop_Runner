using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SHOP_RUNNER.Services.Statistics_Service;

namespace SHOP_RUNNER.Controllers.Statistics_Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private readonly IStatistics _statistic;

        public StatisticController( IStatistics statistics ) { 
            _statistic = statistics;
        }


        // xem số lượng tiền đã bán trong tháng
        // số lượng orders đã đặt hàng trong tháng
        // số lượng đơn hàng đã hoàn thành trong háng
        [HttpGet, Authorize(Roles = "Admin")]
        [Route("statistic-month")]
        public IActionResult GetStatistics()
        {
            try
            {
                    return Ok(new
                    {
                        totalPriceInMonth = _statistic.price_month().ToString(),
                        totalOrdersInMonth = _statistic.ordersMonth().ToString(),
                        OrdersSuccessInMonth = _statistic.orderSuccess().ToString()
                    });
                

            }catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        // top 3 sản phẩm bán chạy nhất theo tháng:
        [HttpGet, Authorize(Roles = "Admin")]
        [Route("top3-fastest-sold")]
        public IActionResult fastest_sold()
        {
            try
            {
                return Ok(_statistic.top3Soldest());
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // các sản phẩm đã bán ra trong tháng:
        [HttpGet, Authorize(Roles = "Admin")]
        [Route("product-sold-month")]
        public IActionResult product_sold()
        {
            try
            {
                return Ok(_statistic.product_sold());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Xem sản chi tiết sản phẩm bán ra trong tháng dung cho "product_sold" và "fastest_sold":
        [HttpGet, Authorize(Roles = "Admin")]
        [Route("detail-soldProduct")]
        public IActionResult detail_sold(int productId)
        {
            try
            {
                return Ok(_statistic.product_detail(productId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // thống kê lấy các mức mua hàng của những tháng trước

        [HttpGet, Authorize(Roles = "Admin")]
        [Route("month-ago")]
        public IActionResult month_ago()
        {

            return Ok(new
            {
                twoMonthAgo = _statistic.price_last_month2().ToString(),
                oneMonthAgo = _statistic.price_last_month().ToString(),
            });  
              
        }


        // THỐNG KÊ TUẦN:

        [HttpGet, Authorize(Roles = "Admin")]
        [Route("statistic-week")]
        public IActionResult GetWeek()
        {
            try
            {
                return Ok(new
                {
                    totalPriceWeek = _statistic.pice_week().ToString(),
                    totalOrdersInWeek = _statistic.ordersWeek().ToString(),
                    successOrderWeek = _statistic.orderSuccessWeek().ToString(),
                });


            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [HttpGet, Authorize(Roles = "Admin")]
        [Route("product-sold-week")]
        public IActionResult product_sold_week()
        {
            try
            {
                return Ok(_statistic.sold_week());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // ngày:
        [HttpGet, Authorize(Roles = "Admin")]
        [Route("statistic-day")]
        public IActionResult GetDay()
        {
            try
            {
                return Ok(new
                {
                    totalPriceDay = _statistic.pice_day().ToString(),
                    totalOrdersInDay = _statistic.ordersDay().ToString(),
                    successOrderDay = _statistic.orderSuccessDay().ToString(),
                });


            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }


        [HttpGet, Authorize(Roles = "Admin")]
        [Route("product-sold-day")]
        public IActionResult product_sold_day()
        {
            try
            {
                return Ok(_statistic.sold_day());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }






    }
}


// SỬA LẠI HUỶ ORDER RETURN PRODUCT TO STOCK