using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppApi.Database;
using WebAppApi.Database.Interface;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.Products.Query
{
    public class GetAllProducts
    {
        // Creo la query CQRS
        public record GetAllProductsQuery() : IRequest<List<ProductVm>>;

        // Creo l'handler
        public class Handler : IRequestHandler<GetAllProductsQuery, List<ProductVm>>
        {
            private readonly IUnitOfWork _unitOfWork;

            public Handler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            }

            public async Task<List<ProductVm>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    return await _unitOfWork.Context.Products
                        .Where(p => !p.IsDeleted)
                        .Select(p => new ProductVm(
                            p.ProductId,
                            p.ProductName,
                            p.IsDeleted,
                            null)) // CartProducts è null in questo caso
                        .ToListAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Errore durante il recupero dei prodotti: {ex.Message}", ex);
                }
            }
        }


        // Endpoint
        public static void MapGetAllProductsEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/products",
            async ([FromServices] ISender sender) =>
            {
                var query = new GetAllProductsQuery();
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetAllProducts")
            .Produces<List<ProductVm>>(StatusCodes.Status200OK)
            .WithTags("Products");
        }

    }
}
