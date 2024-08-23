using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebAppApi.Contracts.Product;
using WebAppApi.Database;
using FluentValidation;
using WebAppApi.ViewModel;
using Microsoft.AspNetCore.Authorization;
using WebAppApi.Database.Interface;

namespace WebAppApi.Features.Products.Query
{
    public class GetProductById
    {
        // Creo la query CQRS
        public record GetProductByIdQuery(GetProductByIdRequest Request) : IRequest<ProductVm>;

        // Creo l'handler
        public class Handler : IRequestHandler<GetProductByIdQuery, ProductVm>
        {
            private readonly IUnitOfWork _unitOfWork;

            public Handler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            }

            public async Task<ProductVm> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var product = await _unitOfWork.Context.Products.FindAsync(new object[] { request.Request.ProductId }, cancellationToken);
                    if (product is null || product.IsDeleted)
                    {
                        throw new Exception("Prodotto non trovato o cancellato.");
                    }

                    return new ProductVm(
                        product.ProductId,
                        product.ProductName,
                        product.IsDeleted,
                        null); // CartProducts è null in questo caso
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Errore durante il recupero del prodotto: {ex.Message}", ex);
                }
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
