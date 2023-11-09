using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SHOP_RUNNER.Entities;
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

        // SỬA
        [HttpPut]
        [Route("update")]
        public IActionResult updateP([FromForm]EditProduct product)
        {
            try
            {
                #region HANDLE THUMBNAIL

                string path = "wwwroot\\Uploads\\Images";
                string filename = Guid.NewGuid().ToString() + Path.GetExtension(product.Thumbnail.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads/Images");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                // var upload = Path.Combine(  Directory.GetCurrentDirectory(), path, filename );
                var upload = Path.Combine(filePath, filename);

                product.Thumbnail.CopyTo(new FileStream(upload, FileMode.Create));

                // using ( var fileStream = new FileStream(upload, FileMode.Create))
                //{ product.thumbnail.CopyTo(fileStream); }

                string url = $"{Request.Scheme}://{Request.Host}/Uploads/Images/{filename}";

                #endregion



                _IProductRepo.UpdateProduct(product, url);
                return Ok("Update success");
            }catch(Exception ex) {
                return BadRequest(ex.Message);
            }

        }

   
        [HttpPost]
        public IActionResult AddP([FromForm]CreateProduct product) {
            try
            {

                #region HANDLE THUMBNAIL

                string path = "wwwroot\\Uploads\\Images";
                string filename = Guid.NewGuid().ToString() + Path.GetExtension(product.Thumbnail.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads/Images");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                // var upload = Path.Combine(  Directory.GetCurrentDirectory(), path, filename );
                var upload = Path.Combine(filePath, filename);

                product.Thumbnail.CopyTo(new FileStream(upload, FileMode.Create));

                // using ( var fileStream = new FileStream(upload, FileMode.Create))
                //{ product.thumbnail.CopyTo(fileStream); }

                string url = $"{Request.Scheme}://{Request.Host}/Uploads/Images/{filename}";

                #endregion


                return Ok(_IProductRepo.AddProduct(product, url));
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


       
        /* [HttpPost]
        [Route("upload-image")]
        public IActionResult UploadImage([FromForm]IFormFile image) {
            string path = "Uploads/Images";
            string filename = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var upload = Path.Combine(Directory.GetCurrentDirectory(), path, filename);

            image.CopyTo(new FileStream(upload, FileMode.Create));

            string url = $"/Uploads/Images/{filename}";
            return Ok(url);
        }*/



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

        [HttpGet]
        [Route("Paginate")]
        public IActionResult Paginate(int page = 1, int PageSize = 1) 
        {
            try
            {
                return Ok( _IProductRepo.Paging(page, PageSize) );
            }catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("Filter")]
        public IActionResult Filtering(double? to, double? from, string? category, string? gender, string? brand, string? size, string? color)
        {
            try
            {
                return Ok(_IProductRepo.Filter(to, from, category, gender, brand, size, color));
            }catch(Exception ex) { 
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("Sorting")]
        public IActionResult Sorting(string? sortBy)
        {
            try
            {
                return Ok( _IProductRepo.Sort(sortBy));

            }catch(Exception ex) { 
            return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("delete")]
        public IActionResult delete(int id)
        {
            try
            {
                _IProductRepo.DeleteProduct(id);
                return Ok("delete success");
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // TÍNH NĂNG CHỈ SỬ DỤNG CHO STAFF OR ADMIN
        // | , Authorizaed("Admin")
        [HttpPost]
        [Route("toggle-product")]
        public IActionResult toggle_p(int product_id)
        {

            _IProductRepo.turn_off_p(product_id);
            return Ok("manipulate success");

        }


    }
}
