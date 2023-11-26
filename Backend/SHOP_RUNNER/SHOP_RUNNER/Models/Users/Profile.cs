using SHOP_RUNNER.Common;
using System.ComponentModel.DataAnnotations;

namespace SHOP_RUNNER.Models.Users
{
    public class Profile
    {
        private string _tel;
        private string _address;
        private string _city;

        [Required]
        public int Id { get; set; }
        [Required]
        public string tel { get { return _tel; } set { _tel = filter_characters.cleanInput(value); } }
        [Required]
        public string address { get { return _address; } set { _address = filter_characters.cleanInput(value); } }
        [Required]
        public string city { get { return _city; } set { _city = filter_characters.cleanInput(value); } }

    }
}
