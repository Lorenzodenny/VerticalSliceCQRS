using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppApi.Contracts.CartProduct;
using WebAppApi.Database;
using WebAppApi.Database.Interface;
using WebAppApi.Shared;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.CartProducts.Query
{
    public class GetCartProductById
    {
        public record GetCartProductByIdQuery(GetCartProductByIdRequest Request) : IRequest<CartProductVm>;

        public class Validator : AbstractValidator<GetCartProductByIdQuery>
        {
            public Validator()
            {
                RuleFor(x => x.Request.CartProductId).GreaterThan(0).WithMessage("CartProductId deve essere maggiore di 0");
            }
        }

        public class Handler : IRequestHandler<GetCartProductByIdQuery, CartProductVm>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IValidator<GetCartProductByIdQuery> _validator;

            public Handler(IUnitOfWork unitOfWork, IValidator<GetCartProductByIdQuery> validator)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task<CartProductVm> Handle(GetCartProductByIdQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        throw new ValidationException(validationResult.Errors);
                    }

                    var cartProduct = await _unitOfWork.Context.CartProducts
                        .Include(cp => cp.Cart)
                        .ThenInclude(c => c.User)
                        .Include(cp => cp.Product)
                        .FirstOrDefaultAsync(cp => cp.CartProductId == request.Request.CartProductId, cancellationToken);

                    if (cartProduct == null)
                    {
                        throw new Exception("CartProduct not found.");
                    }

                    return MapIntoVm.CartProductToCartProductVm(cartProduct);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Errore durante il recupero del prodotto del carrello: {ex.Message}", ex);
                }
            }
        }

        // Endpoint
        public static void MapGetCartProductByIdEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/cartproducts/{cartProductId:int}",
            async (int cartProductId, [FromServices] ISender sender) =>
            {
                try
                {
                    var query = new GetCartProductByIdQuery(new GetCartProductByIdRequest(cartProductId));
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
            .WithName("GetCartProductById")
            .Produces<CartProductVm>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("CartProduct");
        }
    }
}
