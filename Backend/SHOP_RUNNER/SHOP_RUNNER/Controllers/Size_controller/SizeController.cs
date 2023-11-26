using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SHOP_RUNNER.Common;
using SHOP_RUNNER.DTOs.Category_DTO;
using SHOP_RUNNER.Entities;
using SHOP_RUNNER.Models.name_input;

namespace SHOP_RUNNER.Controllers.Size_controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SizeController : ControllerBase
    {
        readonly private RunningShopContext _context;
        private int Page_size = 10;

        public SizeController(RunningShopContext runningShopContext)
        {
            _context = runningShopContext;
        }


        [HttpGet]
        [Route("size/get-all")]
        public IActionResult GetAll(int page = 1)
        {
            try
            {
                var cate = _context.Sizes.AsQueryable();

                var result = PaginationList<Size>.Create(cate, page, Page_size);

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
        [Route("size/delete")]
        public IActionResult delete_size(int id)
        {
            try
            {
                var cate = _context.Sizes.FirstOrDefault(c => c.Id == id);

                if (cate != null)
                {
                    _context.Sizes.Remove(cate);
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
        [Route("size/post")]
        public IActionResult post_data(name_input_model data)
        {
            try
            {
                Size cate_new = new Size()
                {
                    Name = data.name
                };
                _context.Sizes.Add(cate_new);
                _context.SaveChanges();
                return Ok("Create successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut, Authorize(Roles = "Admin,STAFF")]
        [Route("size/update")]
        public IActionResult alert_gender(CategoryGetAll data)
        {
            try
            {
                var cate_new = _context.Sizes.FirstOrDefault(c => c.Id == data.Id);

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




    }
}
