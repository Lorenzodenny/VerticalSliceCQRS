using Microsoft.AspNetCore.Identity;

public static class ConfirmEndpoints
{
    public static void MapConfirmEndpoints(this IEndpointRouteBuilder app)
    {

        // Conferma email
        app.MapGet("/api/users/confirm", async (string userId, string token, UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return Results.NotFound("Utente non trovato.");

            if (user.ConfirmationToken == token && user.TokenExpiryDate > DateTime.UtcNow)
            {
                user.EmailConfirmed = true;
                user.ConfirmationToken = "TOKEN_USED"; // Imposta il token come "TOKEN_USED"
                user.TokenExpiryDate = DateTime.UtcNow; // Imposta la data di scadenza al passato
                await userManager.UpdateAsync(user);
                return Results.Ok("Email confermata con successo.");
            }

            return Results.BadRequest("Token non valido o scaduto.");
        })
        .WithName("ConfirmUserEmail")
        .WithTags("Authentication");

        // Conferma aggiornamento
        app.MapGet("/api/users/confirmUpdate", async (string userId, string token, UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return Results.NotFound("Utente non trovato.");

            if (user.ConfirmationToken == token && user.TokenExpiryDate > DateTime.UtcNow)
            {
                user.ConfirmationToken = "TOKEN_USED"; // Imposta il token come "TOKEN_USED"
                user.TokenExpiryDate = DateTime.UtcNow; // Imposta la data di scadenza al passato
                await userManager.UpdateAsync(user);
                return Results.Ok("Aggiornamento confermato con successo.");
            }

            return Results.BadRequest("Token non valido o scaduto.");
        })
        .WithName("ConfirmUpdate")
        .WithTags("Authentication");

        // Conferma cancellazione
        app.MapGet("/api/users/confirmDelete", async (string userId, string token, UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return Results.NotFound("Utente non trovato.");

            if (user.ConfirmationToken == token && user.TokenExpiryDate > DateTime.UtcNow)
            {
                user.ConfirmationToken = "TOKEN_USED"; // Imposta il token come "TOKEN_USED"
                user.TokenExpiryDate = DateTime.UtcNow; // Imposta la data di scadenza al passato

                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                    return Results.Ok("Cancellazione confermata!");

                return Results.BadRequest("Errore durante la cancellazione.");
            }

            return Results.BadRequest("Token non valido o scaduto.");
        })
        .WithName("ConfirmDelete")
        .WithTags("Authentication");
    }
}
