
using System.ComponentModel.DataAnnotations;

namespace SHOP_RUNNER.Models.Email
{
    public class EmailModel
    {
        [Required, EmailAddress]
        public string to { get; set; } = string.Empty;
        public string subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;


    }
}
