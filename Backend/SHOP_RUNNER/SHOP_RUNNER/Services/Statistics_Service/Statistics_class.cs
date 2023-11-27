using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SHOP_RUNNER.Entities;
using SHOP_RUNNER.Models.Statistics;
using System.Linq.Expressions;

namespace SHOP_RUNNER.Services.Statistics_Service
{
    public class Statistics_class : IStatistics
    {

        readonly private RunningShopContext _context;

        public Statistics_class(RunningShopContext context)
        {
            _context = context;
        }

        public decimal price_last_month()
        {
            try
            {
                Caculate_time a = caculate();

                //int b = a.Year;
                string year = a.Year.ToString();
                string month = a.Month.ToString();
                var parameters = new[]
              {
                new SqlParameter("@year", year),
                new SqlParameter("@month", month)
            };

                var result = _context.total_Months.FromSqlRaw("SELECT SUM(grand_total) as TotalAmount FROM orders WHERE MONTH(created_at) = @month AND YEAR(created_at) = @year", parameters).FirstOrDefault();

                if (result == null)
                {
                    return 0;
                }

                decimal totalAmount = result.TotalAmount;
                return totalAmount;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public decimal price_last_month2()
        {

            try
            {
                Caculate_time a = caculate2();

                //int b = a.Year;
                string year = a.Year.ToString();
                string month = a.Month.ToString();
                var parameters = new[]
              {
                new SqlParameter("@year", year),
                new SqlParameter("@month", month)
            };

                var result = _context.total_Months.FromSqlRaw("SELECT SUM(grand_total) as TotalAmount FROM orders WHERE MONTH(created_at) = @month AND YEAR(created_at) = @year", parameters).FirstOrDefault();

                if (result == null)
                {
                    return 0;
                }

                decimal totalAmount = result.TotalAmount;
                return totalAmount;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }



        private Caculate_time caculate()
        {

            DateTime time_month = DateTime.Now;
            string year = time_month.ToString("yyyy");
            string month = time_month.ToString("MM");
            int m = int.Parse(month);
            int y = int.Parse(year);
            if (m == 1)
            {
                m = 12;
                y -= 1;
                return new Caculate_time()
                {
                    Year = y,
                    Month = m,
                };
            }
            return new Caculate_time()
            {
                Year = y,
                Month = m - 1,
            };
        }

        private Caculate_time caculate2()
        {

            DateTime time_month = DateTime.Now;
            string year = time_month.ToString("yyyy");
            string month = time_month.ToString("MM");
            int m = int.Parse(month);
            int y = int.Parse(year);
            if (m == 1)
            {
                m = 11;
                y -= 1;
                return new Caculate_time()
                {
                    Year = y,
                    Month = m,
                };
            }
            return new Caculate_time()
            {
                Year = y,
                Month = m - 2,
            };
        }


        public decimal price_month()
        {
            DateTime time_month = DateTime.Now;

            // string current_month = time_month.ToString("MM");
            string year = time_month.ToString("yyyy");
            string month = time_month.ToString("MM");
            var parameters = new[]
            {
                new SqlParameter("@year", year),
                new SqlParameter("@month", month)
            };




            //  var result = _context.Set<total_month>().FromSqlRaw("SELECT SUM(grand_total) as total_amount FROM orders WHERE CAST(created_at AS DATE) >= DATEADD(MONTH, -1, @currentDate);", dataParam).FirstOrDefault();

            // cấu hình thêm bảng dbset class bên RunningshopContext:
            // var result = _context.total_Months.FromSqlRaw("SELECT SUM(grand_total) as TotalAmount FROM orders WHERE CAST(created_at AS DATE) >= DATEADD(MONTH, -1, @currentDate)", dataParam).FirstOrDefault();

            var result = _context.total_Months.FromSqlRaw("SELECT SUM(grand_total) as TotalAmount FROM orders WHERE MONTH(created_at) = @month AND YEAR(created_at) = @year", parameters).FirstOrDefault();

            if (result == null)
            {
                return 0;
            }

            decimal totalAmount = result.TotalAmount;
            return totalAmount;
        }

        public int ordersMonth()
        {
            DateTime time_month = DateTime.Now;

            string year = time_month.ToString("yyyy");
            string month = time_month.ToString("MM");
            var parameters = new[]
            {
                new SqlParameter("@year", year),
                new SqlParameter("@month", month)
            };


            var result = _context.orders_A_Months.FromSqlRaw("SELECT COUNT(*) as total_order FROM orders WHERE MONTH(created_at) = @month AND YEAR(created_at) = @year", parameters).FirstOrDefault();

            // VẪN MẮC PHẢI LỖI SAI HÔM QUA -> Khi thực hiện tạo 1 class "orders_A_Months" là trường dữ liệu đó phải chứa đúng tên thuộc tính mà trên DB trả về:
            // trên DB trả về total_order dưới class lại là Order_total bị sai! vì giá trị ko vào trường dữ liệu


            if (result == null)
            {
                return 0;
            }

            int total_orders = result.total_order;

            return total_orders;
        }


        /*
        public int orderSuccess()
        {
            DateTime time_month = DateTime.Now;
            string currentDate = time_month.ToString("yyyy/MM/dd");
            var dataParam = new SqlParameter("@currentDate", currentDate);
            var result = _context.orders_A_Months.FromSqlRaw("SELECT COUNT(*) as total_order FROM orders 
                            WHERE CAST(created_at AS DATE) >= 
                            DATEADD(MONTH, -1, @currentDate) AND status_id = 4 
                            AND shiping_id = 3", dataParam)
                            .FirstOrDefault();

        }
        */

        public int orderSuccess()
        {
            DateTime time_month = DateTime.Now;

            string year = time_month.ToString("yyyy");
            string month = time_month.ToString("MM");
            var parameters = new[]
            {
                new SqlParameter("@year", year),
                new SqlParameter("@month", month)
            };

            var result = _context.orders_A_Months.FromSqlRaw("SELECT COUNT(*) as total_order FROM orders WHERE MONTH(created_at) = @month AND YEAR(created_at) = @year AND status_id = 4 AND shiping_id = 3", parameters).FirstOrDefault();

            // VẪN MẮC PHẢI LỖI SAI HÔM QUA -> Khi thực hiện tạo 1 class "orders_A_Months" là trường dữ liệu đó phải chứa đúng tên thuộc tính mà trên DB trả về:
            // trên DB trả về total_order dưới class lại là Order_total bị sai! vì giá trị ko vào trường dữ liệu
            if (result == null)
            {
                return 0;
            }
            int total_orders = result.total_order;
            return total_orders;
        }


        public List<product_DTO_NAME> top3Soldest()
        {
            DateTime time_month = DateTime.Now;

            string year = time_month.ToString("yyyy");
            string month = time_month.ToString("MM");
            var parameters = new[]
            {
                new SqlParameter("@year", year),
                new SqlParameter("@month", month)
            };


            List<top3soldest> result = _context.top3Soldests.FromSqlRaw("Select TOP 3 od.product_id, SUM(buy_qty) as SELLING FROM orders as o INNER JOIN order_products as od ON o.id = od.order_id WHERE MONTH(created_at) = @month AND YEAR(created_at) = @year GROUP BY od.product_id ORDER BY SELLING DESC", parameters).ToList();

            if (result.Count == 0)
            {
                return null;
            }

            List<product_DTO_NAME> result2 = new List<product_DTO_NAME>(); 

            foreach ( var product in result )
            {
                var p = _context.Products.FirstOrDefault(o => o.Id == product.product_id);
                result2.Add(new product_DTO_NAME()
                {
                    product_id = product.product_id,
                    name = p.Name,
                    price = (p.Price).ToString(),
                    thumbnail = p.Thumbnail,
                    SELLING = product.SELLING
                });
            }

            return result2;

        }


        //tổng số sản phẩm đã bán ra:
        public List<product_DTO_NAME> product_sold()
        {

            DateTime time_month = DateTime.Now;

            string year = time_month.ToString("yyyy");
            string month = time_month.ToString("MM");
            var parameters = new[]
            {
                new SqlParameter("@year", year),
                new SqlParameter("@month", month)
            };


            List<top3soldest> result = _context.top3Soldests.FromSqlRaw("Select od.product_id, SUM(buy_qty) as SELLING FROM orders as o INNER JOIN order_products as od ON o.id = od.order_id WHERE MONTH(created_at) = @month AND YEAR(created_at) = @year GROUP BY od.product_id ORDER BY SELLING DESC", parameters).ToList();



            if (result.Count == 0)
            {
                return null;
            }

            List<product_DTO_NAME> result2 = new List<product_DTO_NAME>();

            foreach (var product in result)
            {
                var p = _context.Products.FirstOrDefault(o => o.Id == product.product_id);
                result2.Add(new product_DTO_NAME()
                {
                    product_id = product.product_id,
                    name = p.Name,
                    price = (p.Price).ToString(),
                    thumbnail = p.Thumbnail,
                    SELLING = product.SELLING
                });
            }

            return result2;

        }


        public List<OrderProduct> product_detail(int productId)
        {
            // .Take(4).OrderByDescending(p => p.Id)
            List<OrderProduct> result = _context.OrderProducts.Where(p => p.ProductId == productId).Take(5).OrderByDescending(p => p.Id).ToList();

            if (result.Count == 0)
            {
                return null;
            }
            return result;

        }


        // THEO TUẦN:
        // tổng tiền trong 1 tuần:
        public decimal pice_week()
        {
            try
            {
                DateTime time_month = DateTime.Now;
                string currentDate = time_month.ToString("yyyy/MM/dd");
                var dataParam = new SqlParameter("@currentDate", currentDate);

                var result = _context.total_Months.FromSqlRaw("SELECT SUM(grand_total) as TotalAmount FROM orders WHERE CAST( created_at AS DATE )>=DATEADD(DAY, -7, @currentDate)", dataParam).FirstOrDefault();

                if (result == null)
                {
                    return 0;
                }

                decimal totalAmount = result.TotalAmount;
                return totalAmount;

            }
            catch (Exception ex)
            {
                return 0;
            }

        }
        // order thành công trong một tuần:
        public int ordersWeek()
        {
            try
            {
                DateTime time_month = DateTime.Now;
                string currentDate = time_month.ToString("yyyy/MM/dd");
                var dataParam = new SqlParameter("@currentDate", currentDate);

                var result = _context.orders_A_Months.FromSqlRaw("SELECT COUNT(*) as total_order FROM orders WHERE CAST( created_at AS DATE )>=DATEADD(DAY, -7, @currentDate)", dataParam).FirstOrDefault();

                // VẪN MẮC PHẢI LỖI SAI HÔM QUA -> Khi thực hiện tạo 1 class "orders_A_Months" là trường dữ liệu đó phải chứa đúng tên thuộc tính mà trên DB trả về:
                // trên DB trả về total_order dưới class lại là Order_total bị sai! vì giá trị ko vào trường dữ liệu
                if (result == null)
                {
                    return 0;
                }
                int total_orders = result.total_order;
                return total_orders;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public int orderSuccessWeek()
        {
            try
            {
                DateTime time_month = DateTime.Now;
                string currentDate = time_month.ToString("yyyy/MM/dd");
                var dataParam = new SqlParameter("@currentDate", currentDate);

                var result = _context.orders_A_Months.FromSqlRaw("SELECT COUNT(*) as total_order FROM orders WHERE CAST( created_at AS DATE )>=DATEADD(DAY, -7, @currentDate) AND status_id = 4 AND shiping_id = 3", dataParam).FirstOrDefault();

                // VẪN MẮC PHẢI LỖI SAI HÔM QUA -> Khi thực hiện tạo 1 class "orders_A_Months" là trường dữ liệu đó phải chứa đúng tên thuộc tính mà trên DB trả về:
                // trên DB trả về total_order dưới class lại là Order_total bị sai! vì giá trị ko vào trường dữ liệu
                if (result == null)
                {
                    return 0;
                }
                int total_orders = result.total_order;
                return total_orders;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public List<top3soldest> sold_week()
        {
            try
            {
                DateTime time_month = DateTime.Now;
                string currentDate = time_month.ToString("yyyy/MM/dd");
                var dataParam = new SqlParameter("@currentDate", currentDate);

                List<top3soldest> result = _context.top3Soldests.FromSqlRaw("Select od.product_id, SUM(buy_qty) as SELLING FROM orders as o INNER JOIN order_products as od ON o.id = od.order_id WHERE CAST( o.created_at AS DATE )>=DATEADD(DAY, -7, @currentDate) GROUP BY od.product_id ORDER BY SELLING DESC", dataParam).ToList();

                if (result.Count == 0)
                {
                    return new List<top3soldest>()
                {
                    new top3soldest()
                    {
                        product_id = 0,
                        SELLING = 0
                    }
                };
                }


                return result;
            }
            catch (Exception ex)
            {
                return new List<top3soldest>()
                {
                    new top3soldest()
                    {
                        product_id = 0,
                        SELLING = 0
                    }
                };
            }
        }


        // THEO NGÀY:
        public decimal pice_day()
        {
            try
            {
                var result = _context.total_Months.FromSqlRaw("SELECT SUM(grand_total) as TotalAmount FROM orders WHERE CAST(created_at AS DATE) = CAST(GETDATE() AS DATE)").FirstOrDefault();

                if (result == null)
                {
                    return 0;
                }

                decimal totalAmount = result.TotalAmount;
                return totalAmount;

            }
            catch (Exception ex)
            {
                return 0;
            }
        }


        public int ordersDay()
        {
            try
            {

                var result = _context.orders_A_Months.FromSqlRaw("SELECT COUNT(*) as total_order FROM orders WHERE CAST(created_at AS DATE) = CAST(GETDATE() AS DATE)").FirstOrDefault();

                // VẪN MẮC PHẢI LỖI SAI HÔM QUA -> Khi thực hiện tạo 1 class "orders_A_Months" là trường dữ liệu đó phải chứa đúng tên thuộc tính mà trên DB trả về:
                // trên DB trả về total_order dưới class lại là Order_total bị sai! vì giá trị ko vào trường dữ liệu
                if (result == null)
                {
                    return 0;
                }
                int total_orders = result.total_order;
                return total_orders;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }


        public int orderSuccessDay()
        {
            try
            {

                var result = _context.orders_A_Months.FromSqlRaw("SELECT COUNT(*) as total_order FROM orders WHERE CAST(created_at AS DATE) = CAST(GETDATE() AS DATE) AND status_id = 4 AND shiping_id = 3").FirstOrDefault();

                // VẪN MẮC PHẢI LỖI SAI HÔM QUA -> Khi thực hiện tạo 1 class "orders_A_Months" là trường dữ liệu đó phải chứa đúng tên thuộc tính mà trên DB trả về:
                // trên DB trả về total_order dưới class lại là Order_total bị sai! vì giá trị ko vào trường dữ liệu
                if (result == null)
                {
                    return 0;
                }
                int total_orders = result.total_order;
                return total_orders;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }


        public List<top3soldest> sold_day()
        {
            try
            {

                List<top3soldest> result = _context.top3Soldests.FromSqlRaw("Select od.product_id, SUM(buy_qty) as SELLING FROM orders as o INNER JOIN order_products as od ON o.id = od.order_id WHERE CAST(created_at AS DATE) = CAST(GETDATE() AS DATE) GROUP BY od.product_id ORDER BY SELLING DESC").ToList();

                if (result.Count == 0)
                {
                    return new List<top3soldest>()
                {
                    new top3soldest()
                    {
                        product_id = 0,
                        SELLING = 0
                    }
                };
                }

                return result;
            }
            catch (Exception ex)
            {
                return new List<top3soldest>()
                {
                    new top3soldest()
                    {
                        product_id = 0,
                        SELLING = 0
                    }
                };
            }
        }




    }
}


// còn sửa lại cập nhật đơn hàng vào stock nhé trong order:
