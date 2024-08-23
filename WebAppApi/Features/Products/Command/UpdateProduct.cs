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
    public class UpdateProduct
    {
        // STEP 1) creo il comando CQRS
        public record UpdateProductCommand(UpdateProductRequest Request) : IRequest<ProductVm>;

        // STEP 2) fluent validator
        public class Validator : AbstractValidator<UpdateProductCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Request.ProductId).GreaterThan(0).WithMessage("ProductId deve essere maggiore di 0");
                RuleFor(x => x.Request.ProductName).NotEmpty().WithMessage("Il nome del prodotto non può essere vuoto");
            }
        }

        // STEP 3) creo l'handler
        public class Handler : IRequestHandler<UpdateProductCommand, ProductVm>
        {
            private readonly eCommerceDbContext _context;
            private readonly IValidator<UpdateProductCommand> _validator;

            public Handler(eCommerceDbContext context, IValidator<UpdateProductCommand> validator)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task<ProductVm> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
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

                existingProduct.ProductName = request.Request.ProductName;

                _context.Products.Update(existingProduct);
                await _context.SaveChangesAsync(cancellationToken);

                return new ProductVm(
                    existingProduct.ProductId,
                    existingProduct.ProductName,
                    existingProduct.IsDeleted,
                    null); // CartProducts rimane lo stesso
            }
        }

        // STEP 4 l'endpoint
        public static void MapUpdateProductEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/products/{id:int}",
            async (int id, UpdateProductCommand command, [FromServices] ISender sender) =>
            {
                try
                {
                    var updatedCommand = command with { Request = command.Request with { ProductId = id } };
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
            .WithName("UpdateProduct")
            .Produces<ProductVm>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Products");
        }



    }
}
