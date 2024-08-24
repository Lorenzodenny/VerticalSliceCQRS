using FluentEmail.Core.Models;

namespace WebAppApi.BackGroundJob
{
    public interface IEmailService
    {
        Task<SendResponse> SendWelcomeEmailAsync(string toEmail, string fullName, string confirmLink);
        Task<SendResponse> SendUpdateConfirmationEmailAsync(string toEmail, string userId);
        Task<SendResponse> SendDeleteConfirmationEmailAsync(string toEmail, string userId);
    }
}
