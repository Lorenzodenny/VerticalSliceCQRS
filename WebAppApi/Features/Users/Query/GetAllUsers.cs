using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppApi.Database;
using WebAppApi.Database.Interface;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.Users
{
    public class GetAllUsers
    {
        // Query CQRS
        public record GetAllUsersQuery : IRequest<List<UserVm>>;

        // Handler
        public class Handler : IRequestHandler<GetAllUsersQuery, List<UserVm>>
        {
            private readonly IUnitOfWork _unitOfWork;

            public Handler(IUnitOfWork unitOfWork)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            }

            public async Task<List<UserVm>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
            {
                try
                {
                    var users = await _unitOfWork.Context.Users
                        .Where(u => !u.IsDeleted)
                        .Include(u => u.Cart)
                        .ToListAsync(cancellationToken);

                    return users.Select(user => new UserVm(
                        user.UserId,
                        user.UserName,
                        user.Email,
                        user.IsDeleted,
                        null // Cart rimane null
                    )).ToList();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Errore durante il recupero degli utenti: {ex.Message}", ex);
                }
            }
        }



        // Endpoint
        public static void MapGetAllUsersEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/users",
            async ([FromServices] ISender sender) =>
            {
                var query = new GetAllUsersQuery();
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetAllUsers")
            .Produces<List<UserVm>>(StatusCodes.Status200OK)
            .WithTags("Users");
        }

    }
}
