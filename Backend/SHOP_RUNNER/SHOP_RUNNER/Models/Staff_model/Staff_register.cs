using System.ComponentModel.DataAnnotations;

namespace SHOP_RUNNER.Models.Staff_model
{
    public class Staff_register
    {
        [Required, MinLength(6)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
