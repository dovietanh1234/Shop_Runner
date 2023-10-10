using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SHOP_RUNNER.Models.Product_Model;
using SHOP_RUNNER.Services.ProductRepo;

namespace SHOP_RUNNER.Controllers.Product
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepo _IProductRepo;

        public ProductController(IProductRepo iProductRepo)
        {
            _IProductRepo = iProductRepo;
        }

        [HttpGet]
        public IActionResult getAll()
        {
            try
            {
                
                return Ok( _IProductRepo.GetAll() );

            }catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("get-by-id")]
        public IActionResult GetDetail(int id)
        {
            try
            {
                var data = _IProductRepo.GetDetail(id);
                return data != null ? Ok(data) : NotFound();

            }catch(Exception ex) { 
                return BadRequest($"{ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public IActionResult updateP(int id, EditProduct product)
        {
            if (id != product.Id)
            {
                return Ok("something went wrong try again");
            }

            try
            {
                _IProductRepo.UpdateProduct(product);
                return Ok("Update success");
            }catch(Exception ex) {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost]
        public IActionResult AddP(CreateProduct product) {
            try
            {
                return Ok(_IProductRepo.AddProduct(product));
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("Search")]
        public IActionResult SearchP(string search)
        {
            try
            {

                if ( search != null )
                {
                    var result = _IProductRepo.Search(search);

                    return result != null?Ok(result) : NotFound();

                }

                return NotFound();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("related")]
        public IActionResult Related(int id)
        {
            try
            {
                var result = _IProductRepo.Relateds(id);
                return result!=null?Ok(result) : NotFound();

            }catch(Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }


    }
}
