using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppApi.Database;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.Carts.Query
{
    public class GetAllCarts
    {
        // Creo la query CQRS
        public record GetAllCartsQuery() : IRequest<List<CartVm>>;

        // Creo l'handler
        public class Handler : IRequestHandler<GetAllCartsQuery, List<CartVm>>
        {
            private readonly eCommerceDbContext _context;

            public Handler(eCommerceDbContext context)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
            }

            public async Task<List<CartVm>> Handle(GetAllCartsQuery request, CancellationToken cancellationToken)
            {
                return await _context.Carts
                    .Where(c => !c.IsDeleted)
                    .Select(c => new CartVm(
                        c.CartId,
                        new UserVm(
                            c.User.UserId,
                            c.User.UserName,
                            c.User.Email,
                            c.User.IsDeleted,
                            null // Puoi lasciare il cart null
                        ),
                        c.IsDeleted,
                        new List<CartProductVm>() // Passa una lista vuota se non ci sono prodotti nel carrello
                    ))
                    .ToListAsync(cancellationToken);
            }
        }

        // L'endpoint
        public static void MapGetAllCartsEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/carts",
            async ([FromServices] ISender sender) =>
            {
                var query = new GetAllCartsQuery();
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetAllCarts")
            .Produces<List<CartVm>>(StatusCodes.Status200OK)
            .WithTags("Carts");
        }
    }
}
