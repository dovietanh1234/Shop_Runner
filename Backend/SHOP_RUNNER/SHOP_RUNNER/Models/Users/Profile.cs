using System.ComponentModel.DataAnnotations;

namespace SHOP_RUNNER.Models.Users
{
    public class Profile
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string tel { get; set; }
        [Required]
        public string address { get; set; }
        [Required]
        public string city { get; set; }

    }
}
