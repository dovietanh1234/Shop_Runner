using SHOP_RUNNER.DTOs.Brand_DTO;
using SHOP_RUNNER.DTOs.Category_DTO;
using SHOP_RUNNER.DTOs.Color_DTO;
using SHOP_RUNNER.DTOs.Gender_DTO;
using SHOP_RUNNER.DTOs.Size_DTO;
using SHOP_RUNNER.Entities;

namespace SHOP_RUNNER.DTOs.Product_DTO
{
    public class ProductDetail
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public decimal Price { get; set; }

        public string? Description { get; set; }

        public string? Thumbnail { get; set; }

        public int Qty { get; set; }

        public int CategoryId { get; set; }

        public DateTime CreateDate { get; set; }

        public int GenderId { get; set; }

        public int BrandId { get; set; }

        public int SizeId { get; set; }

        public int ColorId { get; set; }

        public virtual BrandGetAll Brand { get; set; } = null!;

        public virtual CategoryGetAll Category { get; set; } = null!;

        public virtual ColorGetAll Color { get; set; } = null!;

        public virtual GenderGetAll Gender { get; set; } = null!;

        public virtual SizeGetAll Size { get; set; } = null!;
    }
}
