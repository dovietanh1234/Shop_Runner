using SHOP_RUNNER.DTOs.User_DTO;
using SHOP_RUNNER.Entities;

namespace SHOP_RUNNER.Services.Admin_service
{
    public class Admin_class : IAdmin_repo
    {
        private readonly RunningShopContext _context;
        public Admin_class( RunningShopContext context ) { 
        
            _context = context;
        }

        public List<List_user> get_staffs()
        {
            var result = _context.Users.Where( u => u.Role == "STAFF" ).ToList();

            List<List_user> DTO = new List<List_user>();

            foreach ( var user in result )
            {
                DTO.Add( new List_user()
                {
                    Id = user.Id,
                    Fullname = user.Fullname,
                    Email = user.Email,
                    Role = user.Role,
                    IsActived = user.IsVerified
                });
            }

            return DTO;


        }

        public int toggle_staff(int staff_id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == staff_id);

            if (user == null)
            {
                return 404;
            }

            // nếu là role là staff or admin thì ko thể tắt được:
            if ( user.Role == "Admin")
            {
                return 404;
            }

            if (user.IsVerified == true)
            {
                user.IsVerified = false;
            }
            else
            {
                user.IsVerified = true;
            }

            _context.SaveChanges();
            return 200;
        }




      // toggle product ở bên kia nhá
      // toggle user ở bên kia nhá

    }
}
