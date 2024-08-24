using Hangfire;
using Microsoft.AspNetCore.Identity;
using WebAppApi.BackGroundJob;
using WebAppApi.Identity.Entities;

public static class DeleteUserEndpoint
{
    public static void MapDeleteUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/usersIdentity/{id}", async (string id, UserManager<ApplicationUser> userManager, IEmailService emailService) =>
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                return Results.NotFound();

            // Invia email di conferma per cancellazione
            BackgroundJob.Enqueue(() => emailService.SendDeleteConfirmationEmailAsync(user.Email, user.Id));

            return Results.NoContent();
        })
        .WithName("DeleteUserAccount")
        .WithTags("Authentication");
    }
}