using SHOP_RUNNER.DTOs.Product_DTO;
using SHOP_RUNNER.Models.Product_Model;

namespace SHOP_RUNNER.Services.ProductRepo
{
    public interface IProductRepo
    {
        List<ProductGetAll> GetAll();

        ProductDetail GetDetail(int id);

        ProductGetAll AddProduct(CreateProduct product);

        void UpdateProduct(EditProduct product);

        void DeleteProduct(int id);

        List<ProductGetAll> Search(string search);

        List<ProductGetAll> Relateds(int id);

    }
}
