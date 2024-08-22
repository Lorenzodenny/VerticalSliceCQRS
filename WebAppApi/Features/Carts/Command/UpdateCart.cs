using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAppApi.Contracts.Cart;
using WebAppApi.Database;
using WebAppApi.Entities;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.Carts.Command
{
    public class UpdateCart
    {
        public record UpdateCartCommand(UpdateCartRequest Request) : IRequest<CartVm>;

        public class Validator : AbstractValidator<UpdateCartCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Request.CartId).GreaterThan(0).WithMessage("CartId deve essere maggiore di 0");
                RuleFor(x => x.Request.UserId).GreaterThan(0).WithMessage("UserId deve essere maggiore di 0");
            }
        }

        public class Handler : IRequestHandler<UpdateCartCommand, CartVm>
        {
            private readonly eCommerceDbContext _context;
            private readonly IValidator<UpdateCartCommand> _validator;

            public Handler(eCommerceDbContext context, IValidator<UpdateCartCommand> validator)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task<CartVm> Handle(UpdateCartCommand request, CancellationToken cancellationToken)
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

                existingCart.UserId = request.Request.UserId;

                _context.Carts.Update(existingCart);
                await _context.SaveChangesAsync(cancellationToken);

                // Carica esplicitamente l'utente associato
                var user = await _context.Users.FindAsync(existingCart.UserId);
                if (user == null)
                {
                    throw new Exception("User not found.");
                }

                return new CartVm(
                    existingCart.CartId,
                    new UserVm(user.UserId, user.UserName, user.Email, user.IsDeleted, null),
                    existingCart.IsDeleted,
                    null
                );
            }

        }

        public static void MapUpdateCartEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/carts",
            async (UpdateCartCommand command, [FromServices] ISender sender) =>
            {
                try
                {
                    var result = await sender.Send(command);
                    return Results.Ok(result);
                }
                catch (ValidationException ex)
                {
                    var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
                    return Results.BadRequest(new { Errors = errors });
                }
            })
            .WithName("UpdateCart")
            .Produces<CartVm>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
        }
    }
}
