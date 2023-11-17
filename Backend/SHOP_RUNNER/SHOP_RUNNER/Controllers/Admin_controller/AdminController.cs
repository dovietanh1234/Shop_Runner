using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SHOP_RUNNER.Services.Admin_service;

namespace SHOP_RUNNER.Controllers.Admin_controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {

        private readonly IAdmin_repo _repo;

        public AdminController( IAdmin_repo repo ) { 
        _repo = repo;
        }


        [HttpGet, Authorize(Roles = "Admin")]
        [Route("get-staffs")]
        public IActionResult GetStaffs()
        {
            try
            {

                return Ok( _repo.get_staffs() );

            }catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost, Authorize(Roles = "Admin")]
        [Route("toggle-staffs")]
        public IActionResult toggleStaff(int id)
        {
            try
            {

                return _repo.toggle_staff(id) == 404? NotFound("not found staff "):Ok("change status Success");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
