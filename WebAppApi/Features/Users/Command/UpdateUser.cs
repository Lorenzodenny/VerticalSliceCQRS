using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppApi.Contracts.User;
using WebAppApi.Database;
using WebAppApi.Database.Interface;
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
            private readonly IUnitOfWork _unitOfWork;
            private readonly IValidator<UpdateUserCommand> _validator;

            public Handler(IUnitOfWork unitOfWork, IValidator<UpdateUserCommand> validator)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task<UserVm> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        throw new ValidationException(validationResult.Errors);
                    }

                    var existingUser = await _unitOfWork.Context.Users.FindAsync(new object[] { request.Request.UserId }, cancellationToken);
                    if (existingUser == null)
                    {
                        throw new KeyNotFoundException("Utente non trovato");
                    }

                    existingUser.UserName = request.Request.UserName;
                    existingUser.Email = request.Request.Email;

                    _unitOfWork.Context.Users.Update(existingUser);
                    await _unitOfWork.CompleteAsync(cancellationToken);

                    return new UserVm(
                        existingUser.UserId,
                        existingUser.UserName,
                        existingUser.Email,
                        existingUser.IsDeleted,
                        null); // Cart rimane null
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Errore durante l'aggiornamento dell'utente: {ex.Message}", ex);
                }
            }
        }

        // Endpoint
        public static void MapUpdateUserEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/users/{id:int}",
            async (int id, UpdateUserCommand command, [FromServices] ISender sender) =>
            {
                try
                {
                    var updatedCommand = command with { Request = command.Request with { UserId = id } };
                    var result = await sender.Send(updatedCommand);
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
            .RequireAuthorization()
            .WithName("UpdateUser")
            .Produces<UserVm>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Users");
        }


    }
}
