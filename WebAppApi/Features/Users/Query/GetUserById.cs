using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppApi.Contracts.User;
using WebAppApi.Database;
using WebAppApi.ViewModel;

namespace WebAppApi.Features.Users
{
    public class GetUserById
    {
        // Query CQRS
        public record GetUserByIdQuery(GetUserByIdRequest Request) : IRequest<UserVm>;

        // Fluent Validator
        public class Validator : AbstractValidator<GetUserByIdQuery>
        {
            public Validator()
            {
                RuleFor(x => x.Request.UserId).GreaterThan(0).WithMessage("UserId deve essere maggiore di 0");
            }
        }

        // Handler
        public class Handler : IRequestHandler<GetUserByIdQuery, UserVm>
        {
            private readonly eCommerceDbContext _context;
            private readonly IValidator<GetUserByIdQuery> _validator;

            public Handler(eCommerceDbContext context, IValidator<GetUserByIdQuery> validator)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            }

            public async Task<UserVm> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
            {
                // Esegui la validazione
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    throw new ValidationException(validationResult.Errors);
                }

                var user = await _context.Users
                    .Include(u => u.Cart)
                    .FirstOrDefaultAsync(u => u.UserId == request.Request.UserId && !u.IsDeleted, cancellationToken);

                if (user == null)
                {
                    throw new KeyNotFoundException("Utente non trovato");
                }

                return new UserVm(
                    user.UserId,
                    user.UserName,
                    user.Email,
                    user.IsDeleted,
                    null); // Cart rimane null
            }
        }

        // Endpoint
        public static void MapGetUserByIdEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/users/{id}",
            async (int id, [FromServices] ISender sender) =>
            {
                try
                {
                    var query = new GetUserByIdQuery(new GetUserByIdRequest(id));
                    var result = await sender.Send(query);
                    return result != null ? Results.Ok(result) : Results.NotFound();
                }
                catch (ValidationException ex)
                {
                    var errors = ex.Errors.Select(e => e.ErrorMessage).ToList();
                    return Results.BadRequest(new { Errors = errors });
                }
            })
            .RequireAuthorization()
            .WithName("GetUserById")
            .Produces<UserVm>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Users");
        }

    }
}
