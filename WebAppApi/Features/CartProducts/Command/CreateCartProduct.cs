using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppApi.Contracts.CartProduct;
using WebAppApi.Database;
using WebAppApi.Database.Interface;
using WebAppApi.Entities;
using WebAppApi.Shared;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.CartProducts.Command
{
    public class CreateCartProduct
    {
        public record CreateCartProductCommand(CreateCartProductRequest Request) : IRequest<CartProductVm>;

        public class Validator : AbstractValidator<CreateCartProductCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Request.CartId).GreaterThan(0).WithMessage("CartId deve essere maggiore di 0");
                RuleFor(x => x.Request.ProductId).GreaterThan(0).WithMessage("ProductId deve essere maggiore di 0");
                RuleFor(x => x.Request.Quantity).GreaterThan(0).WithMessage("Quantity deve essere maggiore di 0");
            }
        }

        public class Handler : IRequestHandler<CreateCartProductCommand, CartProductVm>
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IValidator<CreateCartProductCommand> _validator;

            public Handler(IUnitOfWork unitOfWork, IValidator<CreateCartProductCommand> validator)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task<CartProductVm> Handle(CreateCartProductCommand request, CancellationToken cancellationToken)
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                    if (!validationResult.IsValid)
                    {
                        throw new ValidationException(validationResult.Errors);
                    }

                    // Verifica se la combinazione CartId-ProductId esiste già
                    var existingCartProduct = await _unitOfWork.Context.CartProducts
                        .FirstOrDefaultAsync(cp => cp.CartId == request.Request.CartId && cp.ProductId == request.Request.ProductId, cancellationToken);

                    if (existingCartProduct != null)
                    {
                        throw new Exception("Questo prodotto è già presente nel carrello.");
                    }

                    var newCartProduct = new CartProduct
                    {
                        CartId = request.Request.CartId,
                        ProductId = request.Request.ProductId,
                        Quantity = request.Request.Quantity
                    };

                    _unitOfWork.Context.CartProducts.Add(newCartProduct);
                    await _unitOfWork.CompleteAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);

                    // Carica i dati completi del prodotto e del carrello associato
                    var cartProduct = await _unitOfWork.Context.CartProducts
                        .Include(cp => cp.Cart)
                        .ThenInclude(c => c.User)
                        .Include(cp => cp.Product)
                        .FirstOrDefaultAsync(cp => cp.CartProductId == newCartProduct.CartProductId, cancellationToken);

                    return MapIntoVm.CartProductToCartProductVm(cartProduct);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }
            }
        }

        // ENDpoint
        public static void MapCreateCartProductEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/cartproducts",
            async (CreateCartProductCommand command, [FromServices] ISender sender) =>
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
            .RequireAuthorization()
            .WithName("CreateCartProduct")
            .Produces<CartProductVm>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .WithTags("CartProduct");
        }
    }
}
