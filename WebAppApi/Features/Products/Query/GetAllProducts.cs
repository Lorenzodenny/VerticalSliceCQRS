using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppApi.Database;
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
            private readonly eCommerceDbContext _context;

            public Handler(eCommerceDbContext context)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
            }

            public async Task<List<ProductVm>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
            {
                return await _context.Products
                    .Where(p => !p.IsDeleted)
                    .Select(p => new ProductVm(
                        p.ProductId,
                        p.ProductName,
                        p.IsDeleted,
                        null)) // CartProducts è null in questo caso
                    .ToListAsync(cancellationToken);
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
