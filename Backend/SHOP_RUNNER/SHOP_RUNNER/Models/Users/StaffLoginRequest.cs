﻿using System.ComponentModel.DataAnnotations;

namespace SHOP_RUNNER.Models.Users
{
    public class StaffLoginRequest
    {
        [Required, MinLength(6)]
        public string FullName { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
