using SHOP_RUNNER.Common;
using System.ComponentModel.DataAnnotations;

namespace SHOP_RUNNER.Models.Product_Model
{
    public class CreateProduct
    {
        private string _name;
        private string _description;

        [Required(ErrorMessage = "please enter product name")]
        [MinLength(3, ErrorMessage = "enter min 3 character ")]
        [MaxLength(255, ErrorMessage = "enter max 255 character")]
        public string Name { get { return _name; } set { _name = filter_characters.cleanInput(value); } }

        [Required(ErrorMessage = "please enter price product")]
        public int price { get; set; }

        public string description { get { return _description; } set { _description = filter_characters.cleanInput(value); } }

        public IFormFile Thumbnail { get; set; }

        [Required(ErrorMessage = "please enter quantity")]
        public int qty { get; set; }

        [Required(ErrorMessage = "please enter type of category")]
        public int category_id { get; set; }

        public int user_id { get; set; }

        public int gender_id { get; set; }

        public int brand_id { get; set; }

        public int size_id { get; set; }

        public int color_id { get; set; }

    }
}
