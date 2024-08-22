using WebAppApi.Entities;
using WebAppApi.ViewModel;

namespace WebAppApi.Shared
{
    public class MapIntoVm
    {
        public static CartVm CartToCartVm(Cart cart, bool mapUser = true)
        {
            return new CartVm(
                cart.CartId,
                mapUser ? UserToUserVm(cart.User, false) : null, // Evita la ricorsione passando false
                cart.IsDeleted,
                cart.CartProducts.Select(cp => CartProductToCartProductVm(cp, false)).ToList() // Passa false per evitare ricorsione
            );
        }

        public static UserVm UserToUserVm(User user, bool mapCart = true)
        {
            return new UserVm(
                user.UserId,
                user.UserName,
                user.Email,
                user.IsDeleted,
                mapCart ? CartToCartVm(user.Cart, false) : null // Passa false per evitare ricorsione
            );
        }

        public static ProductVm ProductToProductVm(Product product)
        {
            return new ProductVm(
                product.ProductId,
                product.ProductName,
                product.IsDeleted,
                product.CartProducts.Select(cp => CartProductToCartProductVm(cp, false)).ToList() // Evita ricorsione completa
            );
        }

        public static CartProductVm CartProductToCartProductVm(CartProduct cartProduct, bool deepMap = true)
        {
            return new CartProductVm(
                cartProduct.CartProductId,
                cartProduct.CartId,
                cartProduct.ProductId,
                deepMap ? CartToCartVm(cartProduct.Cart) : null, 
                deepMap ? ProductToProductVm(cartProduct.Product) : null, 
                cartProduct.Quantity
            );
        }
    }
}
