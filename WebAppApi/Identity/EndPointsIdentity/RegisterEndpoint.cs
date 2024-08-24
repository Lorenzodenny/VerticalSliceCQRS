using FluentValidation;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebAppApi.BackGroundJob;
using WebAppApi.Identity.Entities;

public static class RegisterEndpoint
{
    public static void MapRegisterEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users/register", async (RegisterModel model, UserManager<ApplicationUser> userManager, IValidator<RegisterModel> validator, IEmailService emailService) =>
        {
            // Validazione
            var validationResult = await validator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => new
                {
                    Field = error.PropertyName,
                    Error = error.ErrorMessage
                });

                return Results.BadRequest(new { Errors = errors });
            }

            // Creazione utente
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                ConfirmationToken = Guid.NewGuid().ToString(),  // Generazione token
                TokenExpiryDate = DateTime.UtcNow.AddHours(24)  // Scadenza del token
            };
            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Aggiunta del claim
                if (!string.IsNullOrEmpty(model.FullName))
                {
                    await userManager.AddClaimAsync(user, new Claim("FullName", model.FullName));
                }

                // Invia l'email di benvenuto con il token
                var confirmLink = $"http://localhost:5222/api/users/confirm?userId={user.Id}&token={user.ConfirmationToken}";
                BackgroundJob.Enqueue(() => emailService.SendWelcomeEmailAsync(model.Email, model.FullName, confirmLink));


                return Results.Ok();
            }
            return Results.BadRequest(result.Errors);
        })
        .WithName("RegisterUser")
        .WithTags("Authentication");
    }
}
