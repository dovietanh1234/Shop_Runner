using SHOP_RUNNER.DTOs.Category_DTO;
using SHOP_RUNNER.Entities;

namespace SHOP_RUNNER.DTOs.Product_DTO
{
    public class ProductGetAll
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }
        
        public string Thumbnail { get; set; }

        public int Qty { get; set; }

        public int CategoryId { get; set; }

        public virtual CategoryGetAll Category { get; set; } = null!;

    }
}
