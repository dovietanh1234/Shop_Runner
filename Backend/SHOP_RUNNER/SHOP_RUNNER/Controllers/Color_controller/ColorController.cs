using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SHOP_RUNNER.Common;
using SHOP_RUNNER.DTOs.Category_DTO;
using SHOP_RUNNER.Entities;
using SHOP_RUNNER.Models.name_input;

namespace SHOP_RUNNER.Controllers.Color_controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColorController : ControllerBase
    {
        readonly private RunningShopContext _context;
        private int Page_size = 10;

        public ColorController(RunningShopContext runningShopContext)
        {
            _context = runningShopContext;
        }


        [HttpGet]
        [Route("color/get-all")]
        public IActionResult GetAll(int page = 1)
        {
            try
            {
                var cate = _context.Colors.AsQueryable();

                var result = PaginationList<Color>.Create(cate, page, Page_size);

                List<CategoryGetAll> list_c = new List<CategoryGetAll>();

                foreach (var c in result)
                {
                    list_c.Add(new CategoryGetAll
                    {
                        Id = c.Id,
                        Name = c.Name
                    });
                }

                return Ok(list_c);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // delete cate:
        [HttpPost, Authorize(Roles = "Admin,STAFF")]
        [Route("color/delete")]
        public IActionResult delete_color(int id)
        {
            try
            {
                var cate = _context.Colors.FirstOrDefault(c => c.Id == id);

                if (cate != null)
                {
                    _context.Colors.Remove(cate);
                    _context.SaveChanges();
                    return Ok("delete successfully");
                }

                return NotFound("delete fail bcs not found id");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost, Authorize(Roles = "Admin,STAFF")]
        [Route("color/post")]
        public IActionResult post_data(name_input_model data)
        {
            try
            {
                Color cate_new = new Color()
                {
                    Name = data.name
                };
                _context.Colors.Add(cate_new);
                _context.SaveChanges();
                return Ok("Create successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut, Authorize(Roles = "Admin,STAFF")]
        [Route("color/update")]
        public IActionResult alert_color(CategoryGetAll data)
        {
            try
            {
                var cate_new = _context.Colors.FirstOrDefault(c => c.Id == data.Id);

                if (cate_new == null)
                {
                    return NotFound("not found the: " + data.Id);
                }

                cate_new.Name = data.Name;
                _context.SaveChanges();
                return Ok("update success");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("Method-payment/get-all")]
        public IActionResult get_MethodPayment(int page = 1)
        {
            try
            {
                var cate = _context.MethodPayments.AsQueryable();

                var result = PaginationList<MethodPayment>.Create(cate, page, Page_size);

                List<CategoryGetAll> list_c = new List<CategoryGetAll>();

                foreach (var c in result)
                {
                    list_c.Add(new CategoryGetAll
                    {
                        Id = c.Id,
                        Name = c.Name
                    });
                }

                return Ok(list_c);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
