using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppApi.Contracts.Product;
using WebAppApi.Database;
using WebAppApi.Entities;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.Products.Command
{
    public class DeleteProduct
    {
        // STEP 1) creo il comando CQRS
        public record DeleteProductCommand(DeleteProductRequest Request) : IRequest<ProductVm>;

        // STEP 2) fluent validator
        public class Validator : AbstractValidator<DeleteProductCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Request.ProductId).GreaterThan(0).WithMessage("ProductId deve essere maggiore di 0");
            }
        }

        // STEP 3) creo l'handler
        public class Handler : IRequestHandler<DeleteProductCommand, ProductVm>
        {
            private readonly eCommerceDbContext _context;
            private readonly IValidator<DeleteProductCommand> _validator;

            public Handler(eCommerceDbContext context, IValidator<DeleteProductCommand> validator)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task<ProductVm> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
            {
                // Esegui la validazione
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                var existingProduct = await _context.Products.FindAsync(request.Request.ProductId);
                if (existingProduct is null || existingProduct.IsDeleted)
                {
                    throw new Exception("Product not found or has been deleted.");
                }

                existingProduct.IsDeleted = true;

                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync(cancellationToken);

                return new ProductVm(
                    existingProduct.ProductId,
                    existingProduct.ProductName,
                    existingProduct.IsDeleted,
                    null);
            }
        }

        // STEP 4 l'endpoint
        public static void MapDeleteProductEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/products/{productId:int}",
            async (int productId, [FromServices] ISender sender) =>
            {
                try
                {
                    var command = new DeleteProductCommand(new DeleteProductRequest(productId));
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
            .RequireAuthorization()
            .WithName("DeleteProduct")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Products");
        }

    }
}
