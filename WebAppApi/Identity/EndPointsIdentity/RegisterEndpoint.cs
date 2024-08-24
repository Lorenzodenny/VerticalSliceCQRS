using FluentValidation;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebAppApi.BackGroundJob;
using WebAppApi.Identity.Entities;
using WebAppApi.ViewModel;

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
                ConfirmationToken = Guid.NewGuid().ToString(),
                TokenExpiryDate = DateTime.UtcNow.AddHours(24)
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
                BackgroundJob.Enqueue(() => emailService.SendWelcomeEmailAsync(model.Email, user.Id, user.ConfirmationToken));

                // Restituisce la ViewModel convertita
                var userVm = (RegisterModelVm)model;

                return Results.Ok(userVm);
            }
            return Results.BadRequest(result.Errors);
        })
        .WithName("RegisterUser")
        .WithTags("Authentication");
    }
}
