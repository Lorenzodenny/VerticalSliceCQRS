using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppApi.Database;
using WebAppApi.Database.Interface;
using WebAppApi.Shared;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.CartProducts.Query
{
    public class GetAllCartProducts
    {
        public record GetAllCartProductsQuery() : IRequest<List<CartProductVm>>;

        public class Handler : IRequestHandler<GetAllCartProductsQuery, List<CartProductVm>>
        {
            private readonly IUnitOfWork _unitOfWork;

            public Handler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            }

            public async Task<List<CartProductVm>> Handle(GetAllCartProductsQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var cartProducts = await _unitOfWork.Context.CartProducts
                        .Include(cp => cp.Cart)
                        .ThenInclude(c => c.User)
                        .Include(cp => cp.Product)
                        .ToListAsync(cancellationToken);

                    return cartProducts.Select(cp => MapIntoVm.CartProductToCartProductVm(cp)).ToList();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Errore durante il recupero dei prodotti del carrello: {ex.Message}", ex);
                }
            }
        }


        // Endpoint
        public static void MapGetAllCartProductsEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/cartproducts",
            async ([FromServices] ISender sender) =>
            {
                var query = new GetAllCartProductsQuery();
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetAllCartProducts")
            .Produces<List<CartProductVm>>(StatusCodes.Status200OK)
            .WithTags("CartProduct");

        }
    }
}
