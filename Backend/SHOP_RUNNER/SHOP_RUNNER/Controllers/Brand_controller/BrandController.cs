using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SHOP_RUNNER.Common;
using SHOP_RUNNER.DTOs.Category_DTO;
using SHOP_RUNNER.Entities;
using SHOP_RUNNER.Models.name_input;

namespace SHOP_RUNNER.Controllers.Brand_controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        readonly private RunningShopContext _context;
        private int Page_size = 10;

        public BrandController(RunningShopContext runningShopContext)
        {
            _context = runningShopContext;
        }



        /*
        [HttpGet]
        [Route("test/a/product")]
        public IActionResult testGetAProduct(int productId)
        {
            try
            {
                var product = _context.Products.Include( p => p.Category ).Include(p => p.Size).FirstOrDefault( p => p.Id == productId );

                if( product == null )
                {
                    return NotFound("không tìm thấy sản phẩm");
                }

                return Ok( new
                {
                       name_cate = product.Category.Name,
                       name_size = product.Size.Name
                });

            }catch(Exception ex) { 
                return BadRequest(ex.Message);
            }
        }*/


        [HttpGet]
        [Route("brand/get-all")]
        public IActionResult GetAll(int page = 1)
        {
            try
            {
                var cate = _context.Brands.AsQueryable();

                var result = PaginationList<Brand>.Create(cate, page, Page_size);

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
        [Route("brand/delete")]
        public IActionResult delete_brand(int id)
        {
            try
            {
                var cate = _context.Brands.FirstOrDefault(c => c.Id == id);

                if (cate != null)
                {
                    _context.Brands.Remove(cate);
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
        [Route("brand/post")]
        public IActionResult post_data(name_input_model data)
        {
            try
            {
                Brand cate_new = new Brand()
                {
                    Name = data.name
                };
                _context.Brands.Add(cate_new);
                _context.SaveChanges();
                return Ok("Create successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut, Authorize(Roles = "Admin,STAFF")]
        [Route("brand/update")]
        public IActionResult alert_brand(CategoryGetAll data)
        {
            try
            {
                var cate_new = _context.Brands.FirstOrDefault(c => c.Id == data.Id);

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
