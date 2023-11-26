using SHOP_RUNNER.DTOs.Product_DTO;
using SHOP_RUNNER.Models.Product_Model;

namespace SHOP_RUNNER.Services.ProductRepo
{
    public interface IProductRepo
    {
        List<ProductGetAll> GetAll();

        ProductDetail GetDetail(int id);

        ProductGetAll AddProduct( CreateProduct product, string url);

        ProductGetAll UpdateProduct(EditProduct product, string url);

        void DeleteProduct(int id);

        List<ProductGetAll> Search(string search, int page);

        List<ProductGetAll> Relateds(int id);

        Object Paging(int page, int pagesize);

        List<ProductGetAll> Filter(double? from, double? to, string? category, string? gender, string? brand, string? size, string? color, int page);

        List<ProductGetAll> Sort(string? sortBy, int page);

        void turn_off_p(int p_id);

    }
}
