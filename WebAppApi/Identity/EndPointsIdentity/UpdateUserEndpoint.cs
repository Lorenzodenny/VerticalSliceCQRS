using Hangfire;
using Microsoft.AspNetCore.Identity;
using WebAppApi.BackGroundJob;
using WebAppApi.Identity.Entities;

public static class UpdateUserEndpoint
{
    public static void MapUpdateUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/usersIdentity/{id}", async (string id, RegisterModel model, UserManager<ApplicationUser> userManager, IEmailService emailService) =>
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                return Results.NotFound();

            user.Email = model.Email;
            user.UserName = model.Email;
            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Invia una email di conferma per l'aggiornamento
                BackgroundJob.Enqueue(() => emailService.SendUpdateConfirmationEmailAsync(model.Email, user.Id));

                return Results.NoContent();
            }
            return Results.BadRequest(result.Errors);
        })
        .WithName("UpdateUserAccount")
        .WithTags("Authentication");
    }
}
