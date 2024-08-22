using WebAppApi.Entities;
using WebAppApi.ViewModel;

namespace WebAppApi.Shared
{
    public class MapIntoVm
    {
        public static CartVm CartToCartVm(Cart cart)
        {
            return new CartVm(
                cart.CartId,
                UserToUserVm(cart.User),
                cart.IsDeleted,
                cart.CartProducts.Select(cp => CartProductToCartProductVm(cp)).ToList()
                );
        }

        public static UserVm UserToUserVm(User user)
        {
            return new UserVm(
                user.UserId,
                user.UserName,
                user.Email,
                user.IsDeleted,
                CartToCartVm(user.Cart)
                );
        }

        public static ProductVm ProductToProductVm(Product product)
        {
            return new ProductVm(
                product.ProductId,
                product.ProductName,
                product.IsDeleted,
                product.CartProducts.Select(cp => CartProductToCartProductVm(cp)).ToList()
                );
        }

        public static CartProductVm CartProductToCartProductVm(CartProduct cartProduct)
        {
            return new CartProductVm(
                cartProduct.CartId,
                cartProduct.ProductId,
                CartToCartVm(cartProduct.Cart),
                ProductToProductVm(cartProduct.Product)
                );
        }
    }
}
