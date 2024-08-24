using Microsoft.AspNetCore.Identity;

public static class ConfirmEndpoints
{
    public static void MapConfirmEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users/confirm", async (string userId, string token, UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return Results.NotFound("Utente non trovato.");

            if (user.ConfirmationToken == token && user.TokenExpiryDate > DateTime.UtcNow)
            {
                user.EmailConfirmed = true;
                user.ConfirmationToken = "TOKEN_USED"; // Rimuovi il token dopo l'uso
                user.TokenExpiryDate = DateTime.UtcNow; // Imposta la data di scadenza al passato;
                await userManager.UpdateAsync(user);
                return Results.Ok("Email confermata con successo.");
            }

            return Results.BadRequest("Token non valido o scaduto.");
        })
        .WithName("ConfirmUserEmail")
        .WithTags("Authentication");



        // Conferma aggiornamento
        app.MapGet("/api/users/confirmUpdate", async (string userId, UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return Results.NotFound();

            // Logica aggiuntiva per l'aggiornamento può essere gestita qui

            return Results.Ok("Aggiornamento confermato!");
        })
        .WithName("ConfirmUpdate")
        .WithTags("Authentication");



        // Conferma cancellazione
        app.MapGet("/api/users/confirmDelete", async (string userId, UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return Results.NotFound();

            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
                return Results.Ok("Cancellazione confermata!");

            return Results.BadRequest("Errore durante la cancellazione.");
        })
        .WithName("ConfirmDelete")
        .WithTags("Authentication");
    }
}
