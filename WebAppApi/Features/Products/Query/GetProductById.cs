using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebAppApi.Contracts.Product;
using WebAppApi.Database;
using FluentValidation;
using WebAppApi.ViewModel;
using Microsoft.AspNetCore.Authorization;

namespace WebAppApi.Features.Products.Query
{
    public class GetProductById
    {
        // Creo la query CQRS
        public record GetProductByIdQuery(GetProductByIdRequest Request) : IRequest<ProductVm>;

        // Creo l'handler
        public class Handler : IRequestHandler<GetProductByIdQuery, ProductVm>
        {
            private readonly eCommerceDbContext _context;

            public Handler(eCommerceDbContext context)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
            }

            public async Task<ProductVm> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
            {
                var product = await _context.Products.FindAsync(request.Request.ProductId);
                if (product is null || product.IsDeleted)
                {
                    throw new Exception("Product not found or has been deleted.");
                }

                return new ProductVm(
                    product.ProductId,
                    product.ProductName,
                    product.IsDeleted,
                    null); // CartProducts è null in questo caso
            }
        }

        // Endpoint
        public static void MapGetProductByIdEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/products/{productId:int}",
            async (int productId, [FromServices] ISender sender) =>
            {
                try
                {
                    var query = new GetProductByIdQuery(new GetProductByIdRequest(productId));
                    var result = await sender.Send(query);
                    return result != null ? Results.Ok(result) : Results.NotFound();
                }
                catch (FluentValidation.ValidationException ex)  // Usare FluentValidation.ValidationException
                {
                    var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
                    return Results.BadRequest(new { Errors = errors });
                }
            })
            .RequireAuthorization()
            .WithName("GetProductById")
            .Produces<ProductVm>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Products");
        }


    }
}
