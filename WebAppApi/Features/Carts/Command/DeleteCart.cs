using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppApi.Contracts.Cart;
using WebAppApi.Database;
using WebAppApi.Entities;

namespace WebAppApi.Features.Carts.Command
{
    public class DeleteCart
    {
        public record DeleteCartCommand(DeleteCartRequest Request) : IRequest;

        public class Validator : AbstractValidator<DeleteCartCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Request.CartId).GreaterThan(0).WithMessage("CartId deve essere maggiore di 0");
            }
        }

        public class Handler : IRequestHandler<DeleteCartCommand>
        {
            private readonly eCommerceDbContext _context;
            private readonly IValidator<DeleteCartCommand> _validator;

            public Handler(eCommerceDbContext context, IValidator<DeleteCartCommand> validator)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task Handle(DeleteCartCommand request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                var existingCart = await _context.Carts.FindAsync(request.Request.CartId);
                if (existingCart is null || existingCart.IsDeleted)
                {
                    throw new Exception("Cart not found or has been deleted.");
                }

                existingCart.IsDeleted = true;

                _context.Carts.Update(existingCart);
                await _context.SaveChangesAsync(cancellationToken);
            }

        }

        // Endpoint
        public static void MapDeleteCartEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/carts/{cartId:int}",
            async (int cartId, [FromServices] ISender sender) =>
            {
                try
                {
                    var command = new DeleteCartCommand(new DeleteCartRequest(cartId));
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
            .RequireAuthorization()
            .WithName("DeleteCart")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .WithTags("Carts"); 
        }
    }
}
