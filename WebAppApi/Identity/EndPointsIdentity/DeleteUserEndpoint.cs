using Hangfire;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebAppApi.BackGroundJob;
using WebAppApi.Identity.Entities;

public static class DeleteUserEndpoint
{
    public static void MapDeleteUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/usersIdentity/delete", async (UserManager<ApplicationUser> userManager, IEmailService emailService, HttpContext httpContext) =>
        {
            // Ottieni l'ID dell'utente loggato
            var userId = httpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return Results.NotFound("Utente non trovato.");

            // Genera un nuovo token per confermare la cancellazione
            user.ConfirmationToken = Guid.NewGuid().ToString();
            user.TokenExpiryDate = DateTime.UtcNow.AddHours(24); // Scadenza del token

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Results.BadRequest(updateResult.Errors);
            }

            if(user.Email != null)
            {
                // Invia email di conferma per cancellazione
                BackgroundJob.Enqueue(() => emailService.SendDeleteConfirmationEmailAsync(user.Email, user.Id, user.ConfirmationToken));
            }
           

            return Results.Ok("Email di conferma inviata. Verifica la tua email per confermare la cancellazione.");
        })
        .WithName("DeleteUserAccount")
        .WithTags("Authentication");
    }
}
