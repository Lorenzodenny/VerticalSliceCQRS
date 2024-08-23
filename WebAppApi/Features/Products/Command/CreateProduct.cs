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
    public class CreateProduct
    {
        // STEP 1) creo il comando CQRS
        public record CreateProductCommand(CreateProductRequest Request) : IRequest<ProductVm>;

        // STEP 2) fluent validator
        public class Validator : AbstractValidator<CreateProductCommand>
        {
            public Validator()
            {
                RuleFor(x => x.Request.ProductName).NotEmpty().WithMessage("Il nome del prodotto non può essere vuoto");
            }
        }

        // STEP 3) creo l'handler
        public class Handler : IRequestHandler<CreateProductCommand, ProductVm>
        {
            private readonly eCommerceDbContext _context;
            private readonly IValidator<CreateProductCommand> _validator;

            public Handler(eCommerceDbContext context, IValidator<CreateProductCommand> validator)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task<ProductVm> Handle(CreateProductCommand request, CancellationToken cancellationToken)
            {
                // Esegui la validazione
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    // Gestisci il caso di fallimento della validazione
                    throw new ValidationException(validationResult.Errors);
                }

                var newProduct = new Product
                {
                    ProductName = request.Request.ProductName,
                    IsDeleted = false
                };

                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync(cancellationToken);

                return new ProductVm(
                    newProduct.ProductId,
                    newProduct.ProductName,
                    newProduct.IsDeleted,
                    null); // CartProducts è null quando il prodotto viene creato
            }
        }

        // STEP 4 l'endpoint
        public static void MapCreateProductEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/products",
            async (CreateProductCommand command, [FromServices] ISender sender) =>
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
            .WithName("CreateProduct")
            .Produces<ProductVm>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .WithTags("Products");
        }

    }
}
