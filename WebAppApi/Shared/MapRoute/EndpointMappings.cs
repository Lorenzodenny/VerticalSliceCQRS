using WebAppApi.Features.Users.Command;
using WebAppApi.Features.Users;
using WebAppApi.Features.Products.Command;
using WebAppApi.Features.Products.Query;
using Microsoft.AspNetCore.Mvc;
using WebAppApi.Features.Carts.Command;
using WebAppApi.Features.Carts.Query;
using WebAppApi.Features.CartProducts.Command;
using WebAppApi.Features.CartProducts.Query;
using WebAppApi.Identity;
using Microsoft.AspNetCore.Identity;
using WebAppApi.Features.ExportExcel;
using WebAppApi.Features.ExportPDF;
// using WebAppApi.Features.CartProducts.Query;

public static class EndpointMappings
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        CreateUser.MapCreateUserEndpoint(app);
        UpdateUser.MapUpdateUserEndpoint(app);
        DeleteUser.MapDeleteUserEndpoint(app);
        GetUserById.MapGetUserByIdEndpoint(app);
        GetAllUsers.MapGetAllUsersEndpoint(app);
    }

    public static void MapProductEndpoints(this WebApplication app)
    {
        CreateProduct.MapCreateProductEndpoint(app);
        UpdateProduct.MapUpdateProductEndpoint(app);
        DeleteProduct.MapDeleteProductEndpoint(app);
        GetProductById.MapGetProductByIdEndpoint(app);
        GetAllProducts.MapGetAllProductsEndpoint(app);
    }

    public static void MapCartEndpoints(this WebApplication app)
    {
        CreateCart.MapCreateCartEndpoint(app);
        UpdateCart.MapUpdateCartEndpoint(app);
        DeleteCart.MapDeleteCartEndpoint(app);
        GetCartById.MapGetCartByIdEndpoint(app);
        GetAllCarts.MapGetAllCartsEndpoint(app);
    }

    public static void MapCartProductEndpoints(this WebApplication app)
    {
        CreateCartProduct.MapCreateCartProductEndpoint(app);
        UpdateCartProduct.MapUpdateCartProductEndpoint(app);
        DeleteCartProduct.MapDeleteCartProductEndpoint(app);
        GetCartProductById.MapGetCartProductByIdEndpoint(app);
        GetAllCartProducts.MapGetAllCartProductsEndpoint(app); 
    }

    public static void MapIdentityEndpoints(this WebApplication app)
    {
        RegisterEndpoint.MapRegisterEndpoint(app);
        LoginEndpoint.MapLoginEndpoint(app);
        UpdateUserEndpoint.MapUpdateUserEndpoint(app);
        DeleteUserEndpoint.MapDeleteUserEndpoint(app);
        ConfirmEndpoints.MapConfirmEndpoints(app);
    }

    public static void MapExportEndpoints(this WebApplication app)
    {
        ExportUserCartProductDetailsPdf.MapEndpoint(app);
        ExportUserCartProductDetailsExcel.MapEndpoint(app);
        ExportUserCartProductViewPdf.MapEndpoint(app);
        ExportUserCartProductViewExcel.MapEndpoint(app);
    }


    public static void MapEndpoints(this WebApplication app)
    {
        app.MapUserEndpoints();
        app.MapProductEndpoints();
        app.MapCartEndpoints();
        app.MapCartProductEndpoints();
        app.MapIdentityEndpoints();
        app.MapExportEndpoints();
    }
}
