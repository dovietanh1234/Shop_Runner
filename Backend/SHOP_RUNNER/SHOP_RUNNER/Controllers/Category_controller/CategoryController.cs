using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SHOP_RUNNER.Common;
using SHOP_RUNNER.DTOs.Category_DTO;
using SHOP_RUNNER.Entities;
using SHOP_RUNNER.Models.name_input;

namespace SHOP_RUNNER.Controllers.Category_controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        readonly private RunningShopContext _context;
        private int Page_size = 10;

        public CategoryController(RunningShopContext runningShopContext)
        {
            _context = runningShopContext;
        }


        [HttpGet]
        [Route("category/get-all")]
        public IActionResult GetAll(int page = 1)
        {
            try
            {
                var cate = _context.Categories.AsQueryable();

                var result = PaginationList<Category>.Create(cate, page, Page_size);

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
        [HttpPost]
        [Route("category/delete")]
        public IActionResult delete_cate(int id)
        {
            try
            {
                var cate = _context.Categories.FirstOrDefault(c => c.Id == id);

                if (cate != null)
                {
                    _context.Categories.Remove(cate);
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

        [HttpPost]
        [Route("category/post-cate")]
        public IActionResult post_data(name_input_model data)
        {
            try
            {
                Category cate_new = new Category()
                {
                    Name = data.name
                };
                _context.Categories.Add(cate_new);
                _context.SaveChanges();
                return Ok("Create successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        [Route("update-cart")]
        public IActionResult alert_cart(CategoryGetAll data)
        {
            try
            {
                var cate_new = _context.Categories.FirstOrDefault(c => c.Id == data.Id);

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
