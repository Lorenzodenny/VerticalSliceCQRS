using FluentEmail.Core.Models;
using FluentEmail.Core;
using WebAppApi.BackGroundJob;

public class EmailService : IEmailService
{
    private readonly IFluentEmail _email;

    public EmailService(IFluentEmail email)
    {
        _email = email;
    }

    public async Task<SendResponse> SendWelcomeEmailAsync(string toEmail, string fullName, string confirmLink)
    {
        var email = _email
            .To(toEmail)
            .Subject("Benvenuto su WebAppApi!")
            .Body($"Ciao {fullName}, benvenuto su WebAppApi! Per confermare la tua registrazione, clicca sul seguente link: {confirmLink}");

        return await email.SendAsync();
    }

    public async Task<SendResponse> SendUpdateConfirmationEmailAsync(string toEmail, string userId)
    {
        var confirmLink = $"http://localhost:5222/api/users/confirmUpdate?userId={userId}";
        var email = _email
            .To(toEmail)
            .Subject("Conferma aggiornamento profilo")
            .Body($"Clicca sul seguente link per confermare l'aggiornamento del tuo profilo: {confirmLink}");

        return await email.SendAsync();
    }

    public async Task<SendResponse> SendDeleteConfirmationEmailAsync(string toEmail, string userId)
    {
        var confirmLink = $"http://localhost:5222/api/users/confirmDelete?userId={userId}";
        var email = _email
            .To(toEmail)
            .Subject("Conferma cancellazione profilo")
            .Body($"Clicca sul seguente link per confermare la cancellazione del tuo profilo: {confirmLink}");

        return await email.SendAsync();
    }
}
