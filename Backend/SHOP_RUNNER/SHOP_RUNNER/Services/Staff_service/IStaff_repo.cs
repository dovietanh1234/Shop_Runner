using SHOP_RUNNER.DTOs.User_DTO;
using SHOP_RUNNER.Models.Staff_model;

namespace SHOP_RUNNER.Services.Staff_service
{
    public interface IStaff_repo
    {
        Task<DTO_staff_regis> Register_account(Staff_register request);

        int change_password( Staff_changePass user );

        // toggle san pham ta da lam trong file Product.Controller.cs


        // lay ra cac account user isActive == true && isActive ==false de chan or bat user ...
        List<List_user> list_user();

        int toggleUser(int userId);
        
    }
}
