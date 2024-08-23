using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppApi.Contracts.Cart;
using WebAppApi.Database;
using WebAppApi.Database.Interface;
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
            private readonly IUnitOfWork _unitOfWork;
            private readonly IValidator<UpdateCartCommand> _validator;

            public Handler(IUnitOfWork unitOfWork, IValidator<UpdateCartCommand> validator)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task<CartVm> Handle(UpdateCartCommand request, CancellationToken cancellationToken)
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        throw new ValidationException(validationResult.Errors);
                    }

                    var existingCart = await _unitOfWork.Context.Carts.FindAsync(new object[] { request.Request.CartId }, cancellationToken);
                    if (existingCart is null || existingCart.IsDeleted)
                    {
                        throw new Exception("Carrello non trovato o cancellato.");
                    }

                    existingCart.UserId = request.Request.UserId;
                    _unitOfWork.Context.Carts.Update(existingCart);
                    await _unitOfWork.CompleteAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    // Carica esplicitamente l'utente associato
                    var user = await _unitOfWork.Context.Users.FindAsync(new object[] { existingCart.UserId }, cancellationToken);
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
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            }
        }

        // Endpoint
        public static void MapUpdateCartEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/carts/{id:int}",
            async (int id, UpdateCartCommand command, [FromServices] ISender sender) =>
            {
                try
                {
                    var updatedCommand = command with { Request = command.Request with { CartId = id } };
                    var result = await sender.Send(updatedCommand);
                    return Results.Ok(result);
                }
                catch (ValidationException ex)
                {
                    var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
                    return Results.BadRequest(new { Errors = errors });
                }
            })
            .RequireAuthorization()
            .WithName("UpdateCart")
            .Produces<CartVm>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Carts");
        }


    }
}
