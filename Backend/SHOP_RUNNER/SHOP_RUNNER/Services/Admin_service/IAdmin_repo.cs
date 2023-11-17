using SHOP_RUNNER.DTOs.User_DTO;

namespace SHOP_RUNNER.Services.Admin_service
{
    public interface IAdmin_repo
    {
        List<List_user> get_staffs();

        int toggle_staff(int staff_id);

    }
}
