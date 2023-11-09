using System.ComponentModel.DataAnnotations;

namespace SHOP_RUNNER.Models.Staff_model
{
    public class Staff_changePass
    {
        [Required]
        public int Id { get; set; }

        [Required, MinLength(6)]
        public string Password_old { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password_new { get; set; } = string.Empty;

    }
}
