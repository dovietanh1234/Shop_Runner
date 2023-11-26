using SHOP_RUNNER.Common;
using System.ComponentModel.DataAnnotations;

namespace SHOP_RUNNER.Models.Users
{
    public class UserLoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
