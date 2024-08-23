using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppApi.Contracts.CartProduct;
using WebAppApi.Database;
using WebAppApi.Entities;
using WebAppApi.Shared;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.CartProducts.Command
{
    public class UpdateCartProduct
    {
        public record UpdateCartProductCommand(UpdateCartProductRequest Request) : IRequest<CartProductVm>;

        public class Validator : AbstractValidator<UpdateCartProductCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Request.CartProductId).GreaterThan(0).WithMessage("CartProductId deve essere maggiore di 0");
                RuleFor(x => x.Request.NewCartId).GreaterThan(0).WithMessage("CartId deve essere maggiore di 0");
                RuleFor(x => x.Request.NewProductId).GreaterThan(0).WithMessage("ProductId deve essere maggiore di 0");
                RuleFor(x => x.Request.Quantity).GreaterThan(0).WithMessage("Quantity deve essere maggiore di 0");
            }
        }

        public class Handler : IRequestHandler<UpdateCartProductCommand, CartProductVm>
        {
            private readonly eCommerceDbContext _context;
            private readonly IValidator<UpdateCartProductCommand> _validator;

            public Handler(eCommerceDbContext context, IValidator<UpdateCartProductCommand> validator)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task<CartProductVm> Handle(UpdateCartProductCommand request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                var existingCartProduct = await _context.CartProducts
                    .Include(cp => cp.Cart)
                    .ThenInclude(c => c.User)
                    .Include(cp => cp.Product)
                    .FirstOrDefaultAsync(cp => cp.CartProductId == request.Request.CartProductId);

                if (existingCartProduct == null)
                {
                    throw new Exception("CartProduct not found.");
                }

                // Aggiorna i campi con i nuovi valori
                existingCartProduct.CartId = request.Request.NewCartId;
                existingCartProduct.ProductId = request.Request.NewProductId;
                existingCartProduct.Quantity = request.Request.Quantity;

                _context.CartProducts.Update(existingCartProduct);
                await _context.SaveChangesAsync(cancellationToken);

                return MapIntoVm.CartProductToCartProductVm(existingCartProduct);
            }
        }


        // Endpoint
        public static void MapUpdateCartProductEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/cartproducts/{id:int}",
            async (int id, UpdateCartProductCommand command, [FromServices] ISender sender) =>
            {
                try
                {
                    var updatedCommand = command with { Request = command.Request with { CartProductId = id } };
                    var result = await sender.Send(updatedCommand);
                    return Results.Ok(result);
                }
                catch (ValidationException ex)
                {
                    var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
                    return Results.BadRequest(new { Errors = errors });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            })
            .RequireAuthorization()
            .WithName("UpdateCartProduct")
            .Produces<CartProductVm>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("CartProduct");
        }


    }
}
