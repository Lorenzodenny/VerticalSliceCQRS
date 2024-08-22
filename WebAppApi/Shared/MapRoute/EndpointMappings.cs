using WebAppApi.Features.Users.Command;
using WebAppApi.Features.Users;
using WebAppApi.Features.Products.Command;
using WebAppApi.Features.Products.Query;
using Microsoft.AspNetCore.Mvc;

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


    public static void MapEndpoints(this WebApplication app)
    {
        app.MapUserEndpoints();
        app.MapProductEndpoints();
        // Aggiungi altre entità se necessario
    }
}
