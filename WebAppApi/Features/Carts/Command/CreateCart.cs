using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppApi.Contracts.Cart;
using WebAppApi.Database;
using WebAppApi.Entities;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.Carts.Command
{
    public class CreateCart
    {
        public record CreateCartCommand(CreateCartRequest Request) : IRequest<CartVm>;

        public class Validator : AbstractValidator<CreateCartCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Request.UserId).GreaterThan(0).WithMessage("UserId deve essere maggiore di 0");
            }
        }

        public class Handler : IRequestHandler<CreateCartCommand, CartVm>
        {
            private readonly eCommerceDbContext _context;
            private readonly IValidator<CreateCartCommand> _validator;

            public Handler(eCommerceDbContext context, IValidator<CreateCartCommand> validator)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task<CartVm> Handle(CreateCartCommand request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                // Controlla se esiste già un carrello per questo utente
                var existingCart = await _context.Carts
                    .FirstOrDefaultAsync(c => c.UserId == request.Request.UserId && !c.IsDeleted, cancellationToken);

                if (existingCart != null)
                {
                    throw new Exception("L'utente ha già un carrello attivo.");
                }

                var newCart = new Cart
                {
                    UserId = request.Request.UserId,
                    IsDeleted = false
                };

                _context.Carts.Add(newCart);
                await _context.SaveChangesAsync(cancellationToken);

                // Carica esplicitamente l'utente associato
                var user = await _context.Users.FindAsync(newCart.UserId);
                if (user == null)
                {
                    throw new Exception("User not found.");
                }

                return new CartVm(
                    newCart.CartId,
                    new UserVm(user.UserId, user.UserName, user.Email, user.IsDeleted, null),
                    newCart.IsDeleted, null
                );
            }

        }

        public static void MapCreateCartEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/carts",
            async (CreateCartCommand command, [FromServices] ISender sender) =>
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
            .WithName("CreateCart")
            .Produces<CartVm>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
        }
    }
}
