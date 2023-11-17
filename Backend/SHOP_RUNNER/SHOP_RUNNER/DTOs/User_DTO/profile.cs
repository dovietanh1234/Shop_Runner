using System.ComponentModel.DataAnnotations;

namespace SHOP_RUNNER.DTOs.User_DTO
{
    public class profile
    {
        public string Fullname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string tel { get; set; }
        public string address { get; set; }
        public string city { get; set; }
    }
}
