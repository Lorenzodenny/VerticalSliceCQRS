using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppApi.Contracts.Cart;
using WebAppApi.Database;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.Carts.Query
{
    public class GetCartById
    {
        public record GetCartByIdQuery(GetCartByIdRequest Request) : IRequest<CartVm>;

        public class Handler : IRequestHandler<GetCartByIdQuery, CartVm>
        {
            private readonly eCommerceDbContext _context;

            public Handler(eCommerceDbContext context)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
            }

            public async Task<CartVm> Handle(GetCartByIdQuery request, CancellationToken cancellationToken)
            {
                var cart = await _context.Carts.FindAsync(request.Request.CartId);
                if (cart is null || cart.IsDeleted)
                {
                    throw new Exception("Cart not found or has been deleted.");
                }

                return new CartVm(cart.CartId, new UserVm(cart.User.UserId, cart.User.UserName, cart.User.Email, cart.User.IsDeleted, null), cart.IsDeleted, null);
            }
        }

        // ENdpoint
        public static void MapGetCartByIdEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/carts/{cartId:int}",
            async (int cartId, [FromServices] ISender sender) =>
            {
                try
                {
                    var query = new GetCartByIdQuery(new GetCartByIdRequest(cartId));
                    var result = await sender.Send(query);
                    return result != null ? Results.Ok(result) : Results.NotFound();
                }
                catch (ValidationException ex)
                {
                    var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
                    return Results.BadRequest(new { Errors = errors });
                }
            })
            .RequireAuthorization()
            .WithName("GetCartById")
            .Produces<CartVm>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Carts");
        }
    }
}
