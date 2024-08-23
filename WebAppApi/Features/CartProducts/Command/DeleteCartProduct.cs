using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppApi.Contracts.CartProduct;
using WebAppApi.Database;
using WebAppApi.Database.Interface;

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
            private readonly IUnitOfWork _unitOfWork;
            private readonly IValidator<DeleteCartProductCommand> _validator;

            public Handler(IUnitOfWork unitOfWork, IValidator<DeleteCartProductCommand> validator)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task Handle(DeleteCartProductCommand request, CancellationToken cancellationToken)
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        throw new ValidationException(validationResult.Errors);
                    }

                    var existingCartProduct = await _unitOfWork.Context.CartProducts.FindAsync(new object[] { request.Request.CartProductId }, cancellationToken);
                    if (existingCartProduct is null)
                    {
                        throw new Exception("CartProduct not found.");
                    }

                    _unitOfWork.Context.CartProducts.Remove(existingCartProduct);
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
            .RequireAuthorization()
            .WithName("DeleteCartProduct")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .WithTags("CartProduct");
        }
    }
}
