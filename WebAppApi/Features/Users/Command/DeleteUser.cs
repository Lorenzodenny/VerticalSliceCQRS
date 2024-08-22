using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAppApi.Contracts.User;
using WebAppApi.Database;

namespace WebAppApi.Features.Users.Command
{
    public class DeleteUser
    {
        // Command CQRS
        public record DeleteUserCommand(DeleteUserRequest Request) : IRequest<Unit>;

        // Fluent Validator
        public class Validator : AbstractValidator<DeleteUserCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Request.UserId).GreaterThan(0).WithMessage("UserId deve essere maggiore di 0");
            }
        }

        // Handler
        public class Handler : IRequestHandler<DeleteUserCommand, Unit>
        {
            private readonly eCommerceDbContext _context;
            private readonly IValidator<DeleteUserCommand> _validator;

            public Handler(eCommerceDbContext context, IValidator<DeleteUserCommand> validator)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
            {
                // Esegui la validazione
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                var existingUser = await _context.Users.FindAsync(request.Request.UserId);
                if (existingUser == null)
                {
                    throw new KeyNotFoundException("Utente non trovato");
                }

                existingUser.IsDeleted = true;
                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }

        // Endpoint
        public static void MapDeleteUserEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/users/{id}",
            async (int id, [FromServices] ISender sender) =>
            {
                try
                {
                    var command = new DeleteUserCommand(new DeleteUserRequest(id));
                    await sender.Send(command);
                    return Results.NoContent();
                }
                catch (ValidationException ex)
                {
                    var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
                    return Results.BadRequest(new { Errors = errors });
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            })
            .WithName("DeleteUser")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
        }

    }
}
