using SHOP_RUNNER.DTOs.Cart;
using SHOP_RUNNER.Entities;

namespace SHOP_RUNNER.Services.Cart_service
{
    public interface ICart_m
    {
        int createCart(int userId, int product_id);

        int updateCart(int userId, int product_id, int? plus, int? minus, int? quantity);

        int delete_product(int userId, int product_id);

        int delete_cart(int userId);

         List<GetCart> get_cart(int userId);

    }
}
