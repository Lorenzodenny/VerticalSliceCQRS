using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppApi.Contracts.Cart;
using WebAppApi.Database;
using WebAppApi.Database.Interface;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.Carts.Query
{
    public class GetCartById
    {
        public record GetCartByIdQuery(GetCartByIdRequest Request) : IRequest<CartVm>;

        public class Handler : IRequestHandler<GetCartByIdQuery, CartVm>
        {
            private readonly IUnitOfWork _unitOfWork;

            public Handler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            }

            public async Task<CartVm> Handle(GetCartByIdQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var cart = await _unitOfWork.Context.Carts.FindAsync(new object[] { request.Request.CartId }, cancellationToken);
                    if (cart is null || cart.IsDeleted)
                    {
                        throw new Exception("Carrello non trovato o cancellato.");
                    }

                    return new CartVm(
                        cart.CartId,
                        new UserVm(cart.User.UserId, cart.User.UserName, cart.User.Email, cart.User.IsDeleted, null),
                        cart.IsDeleted,
                        null
                    );
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Errore durante il recupero del carrello: {ex.Message}", ex);
                }
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
