using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAppApi.Contracts.User;
using WebAppApi.Database;
using WebAppApi.Entities;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.Users.Command
{
    public class CreateUser
    {
        // Creo il comando CQRS
        public record CreateUserCommand(CreateUserRequest Request) : IRequest<UserVm>;

        // Fluent Validator
        public class Validator : AbstractValidator<CreateUserCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Request.UserName).NotEmpty().WithMessage("Il nome utente non può essere vuoto");
                RuleFor(x => x.Request.Email).NotEmpty().EmailAddress().WithMessage("Inserisci un'email valida");
            }
        }

        // Creo l'handler
        public class Handler : IRequestHandler<CreateUserCommand, UserVm>
        {
            private readonly eCommerceDbContext _context;
            private readonly IValidator<CreateUserCommand> _validator;

            public Handler(eCommerceDbContext context, IValidator<CreateUserCommand> validator)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task<UserVm> Handle(CreateUserCommand request, CancellationToken cancellationToken)
            {
                // Esegui la validazione
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    // Gestisci il fallimento della validazione
                    throw new ValidationException(validationResult.Errors);
                }

                var newUser = new User
                {
                    UserName = request.Request.UserName,
                    Email = request.Request.Email,
                    IsDeleted = false
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync(cancellationToken);

                return new UserVm(
                    newUser.UserId,
                    newUser.UserName,
                    newUser.Email,
                    newUser.IsDeleted,
                    null); // Cart è null quando l'utente viene creato
            }
        }

        // Endpoint
        public static void MapCreateUserEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/users",
            async (CreateUserCommand command, [FromServices] ISender sender) =>
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
            .WithName("CreateUser")
            .Produces<UserVm>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest);
        }

    }
}
