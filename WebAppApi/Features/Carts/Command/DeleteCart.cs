using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppApi.Contracts.Cart;
using WebAppApi.Database;
using WebAppApi.Database.Interface;
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
            private readonly IUnitOfWork _unitOfWork;
            private readonly IValidator<DeleteCartCommand> _validator;

            public Handler(IUnitOfWork unitOfWork, IValidator<DeleteCartCommand> validator)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task Handle(DeleteCartCommand request, CancellationToken cancellationToken)
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
                        throw new Exception("Carrello non trovato o già cancellato.");
                    }

                    existingCart.IsDeleted = true;
                    _unitOfWork.Context.Carts.Update(existingCart);
                    await _unitOfWork.CompleteAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
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
