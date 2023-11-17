using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SHOP_RUNNER.Models.Staff_model;
using SHOP_RUNNER.Services.Staff_service;
using System.Security.Claims;

namespace SHOP_RUNNER.Controllers.Staff_controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly IStaff_repo _staff_Repo;

        public StaffController(IStaff_repo staff_Repo)
        {
            _staff_Repo = staff_Repo;
        }


        

        [HttpPost, Authorize(Roles = "Admin,STAFF")]
        [Route("create-acc-staff")]
        public async Task<IActionResult> Index(Staff_register request)
        {
            try
            {
                return Ok( new
                {
                    message = "Your Account create success",
                    account = await _staff_Repo.Register_account(request)
                });

            }catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // change password nay co the su dung cho staff and user
        [HttpPost, Authorize(Roles = "Admin,STAFF")]
        [Route("change-password")]
        public IActionResult changePassword(Staff_changePass user_new)
        {
            try
            {

                int a = _staff_Repo.change_password(user_new);
                if (a == 404)
                {
                    return NotFound("account is not exist");
                }
                if ( a == 403 )
                {
                    return Forbid(" fail your account is not verified ");
                }
                if ( a == 401 )
                {
                    return Unauthorized("password wrong");
                }
                return Ok(" change password success ");


            }catch(Exception ex) { 
                return BadRequest(ex.Message);
            }


        }


        [HttpGet, Authorize(Roles = "Admin, STAFF")]
        [Route("get-users")]
        public IActionResult getAllUser()
        {
            try
            {

                return Ok(_staff_Repo.list_user());

            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost, Authorize(Roles = "Admin")]
        [Route("toggle-user")]
        public IActionResult toggle_user(int userId)
        {
            try
            {
                return _staff_Repo.toggleUser(userId) == 404?NotFound("user is not exist"):Ok(" changes done! ");

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




    }
}
