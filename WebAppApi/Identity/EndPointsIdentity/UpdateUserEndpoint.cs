using Hangfire;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebAppApi.BackGroundJob;
using WebAppApi.Identity.Entities;
using WebAppApi.ViewModel;

public static class UpdateUserEndpoint
{
    public static void MapUpdateUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/usersIdentity/update", async (RegisterModel model, string currentPassword, UserManager<ApplicationUser> userManager, IEmailService emailService, HttpContext httpContext) =>
        {
            // Ottieni l'ID dell'utente loggato
            var userId = httpContext.User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;


            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return Results.NotFound("Utente non trovato.");

            // Aggiorna le informazioni solo se fornite 
            if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
            {
                user.Email = model.Email;
                user.UserName = model.Email;
            }

            if (!string.IsNullOrEmpty(model.PhoneNumber) && model.PhoneNumber != user.PhoneNumber)
            {
                user.PhoneNumber = model.PhoneNumber;
            }

            if (!string.IsNullOrEmpty(model.Password))
            {
                if (string.IsNullOrEmpty(currentPassword))
                {
                    return Results.BadRequest("La password attuale è richiesta per modificare la password.");
                }

                var passwordChangeResult = await userManager.ChangePasswordAsync(user, currentPassword, model.Password);
                if (!passwordChangeResult.Succeeded)
                {
                    return Results.BadRequest(passwordChangeResult.Errors);
                }
            }

            // Genera un nuovo token per confermare l'aggiornamento
            user.ConfirmationToken = Guid.NewGuid().ToString();
            user.TokenExpiryDate = DateTime.UtcNow.AddHours(24);

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Invia una email di conferma per l'aggiornamento con il nuovo token
                BackgroundJob.Enqueue(() => emailService.SendUpdateConfirmationEmailAsync(user.Email, user.Id, user.ConfirmationToken));

                // Utilizza il cast esplicito per convertire RegisterModel in RegisterModelVm
                var updatedUserVm = (RegisterModelVm)model;

                return Results.Ok(new { Message = "Email di conferma inviata. Verifica la tua email per confermare l'aggiornamento.", User = updatedUserVm });
            }

            return Results.BadRequest(result.Errors);
        })
        .WithName("UpdateUserAccount")
        .WithTags("Authentication");
    }
}
