using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAppApi.Contracts.User;
using WebAppApi.Database;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.Users.Command
{
    public class UpdateUser
    {
        // Command CQRS
        public record UpdateUserCommand(UpdateUserRequest Request) : IRequest<UserVm>;

        // Fluent Validator
        public class Validator : AbstractValidator<UpdateUserCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Request.UserId).GreaterThan(0).WithMessage("UserId deve essere maggiore di 0");
                RuleFor(x => x.Request.UserName).NotEmpty().WithMessage("Inserisci un nome Utente");
                RuleFor(x => x.Request.Email).NotEmpty().EmailAddress().WithMessage("Inserisci una e-mail valida");
            }
        }

        // Handler
        public class Handler : IRequestHandler<UpdateUserCommand, UserVm>
        {
            private readonly eCommerceDbContext _context;
            private readonly IValidator<UpdateUserCommand> _validator;

            public Handler(eCommerceDbContext context, IValidator<UpdateUserCommand> validator)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task<UserVm> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
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

                existingUser.UserName = request.Request.UserName;
                existingUser.Email = request.Request.Email;

                await _context.SaveChangesAsync(cancellationToken);

                return new UserVm(
                    existingUser.UserId,
                    existingUser.UserName,
                    existingUser.Email,
                    existingUser.IsDeleted,
                    null); // Cart rimane null
            }
        }

        // Endpoint
        public static void MapUpdateUserEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/users",
            async (UpdateUserCommand command, [FromServices] ISender sender) =>
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
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            })
            .WithName("UpdateUser")
            .Produces<UserVm>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
        }

    }
}
