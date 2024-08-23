using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppApi.Database;
using WebAppApi.Shared;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.CartProducts.Query
{
    public class GetAllCartProducts
    {
        public record GetAllCartProductsQuery() : IRequest<List<CartProductVm>>;

        public class Handler : IRequestHandler<GetAllCartProductsQuery, List<CartProductVm>>
        {
            private readonly eCommerceDbContext _context;

            public Handler(eCommerceDbContext context)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
            }

            public async Task<List<CartProductVm>> Handle(GetAllCartProductsQuery request, CancellationToken cancellationToken)
            {
                var cartProducts = await _context.CartProducts
                    .Include(cp => cp.Cart)
                    .ThenInclude(c => c.User)
                    .Include(cp => cp.Product)
                    .ToListAsync(cancellationToken);

                return cartProducts.Select(cp => MapIntoVm.CartProductToCartProductVm(cp)).ToList();
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
