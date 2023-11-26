using SHOP_RUNNER.Common;
using System.ComponentModel.DataAnnotations;

namespace SHOP_RUNNER.Models.Product_Model
{
    public class EditProduct
    {
        private string _name;
        private string _description;

        public int Id { get; set; }

        [Required(ErrorMessage = "please enter product name")]
        [MinLength(3, ErrorMessage = "enter min 3 character ")]
        [MaxLength(255, ErrorMessage = "enter max 255 character")]
        public string Name { get; set; }

        [Required(ErrorMessage = "please enter price product")]
        public int price { get; set; }

        public string description { get; set; } 

        public IFormFile Thumbnail { get; set; } = null;

        [Required(ErrorMessage = "please enter quantity")]
        public int qty { get; set; }

        [Required(ErrorMessage = "please enter type of category")]
        public int categoryId { get; set; }

        public int genderId { get; set; }

        public int brandId { get; set; }

        public int sizeId { get; set; }

        public int colorId { get; set; }
    }
}
