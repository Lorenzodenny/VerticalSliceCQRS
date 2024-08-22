using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAppApi.Contracts.CartProduct;
using WebAppApi.Database;

namespace WebAppApi.Features.CartProducts.Command
{
    public class DeleteCartProduct
    {
        public record DeleteCartProductCommand(DeleteCartProductRequest Request) : IRequest;

        public class Validator : AbstractValidator<DeleteCartProductCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Request.CartProductId).GreaterThan(0).WithMessage("CartProductId deve essere maggiore di 0");
            }
        }

        public class Handler : IRequestHandler<DeleteCartProductCommand>
        {
            private readonly eCommerceDbContext _context;
            private readonly IValidator<DeleteCartProductCommand> _validator;

            public Handler(eCommerceDbContext context, IValidator<DeleteCartProductCommand> validator)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task Handle(DeleteCartProductCommand request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                var existingCartProduct = await _context.CartProducts.FindAsync(request.Request.CartProductId);
                if (existingCartProduct is null)
                {
                    throw new Exception("CartProduct not found.");
                }

                _context.CartProducts.Remove(existingCartProduct);
                await _context.SaveChangesAsync(cancellationToken);

            }
        }

        public static void MapDeleteCartProductEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/cartproducts/{cartProductId:int}",
            async (int cartProductId, [FromServices] ISender sender) =>
            {
                try
                {
                    var command = new DeleteCartProductCommand(new DeleteCartProductRequest(cartProductId));
                    await sender.Send(command);
                    return Results.NoContent();
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
            .WithName("DeleteCartProduct")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
        }
    }
}
