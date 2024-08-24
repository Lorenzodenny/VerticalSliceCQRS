using FluentEmail.Core.Models;

namespace WebAppApi.BackGroundJob
{
    public interface IEmailService
    {
        Task<SendResponse> SendWelcomeEmailAsync(string toEmail, string userId, string token);
        Task<SendResponse> SendUpdateConfirmationEmailAsync(string toEmail, string userId, string token);
        Task<SendResponse> SendDeleteConfirmationEmailAsync(string toEmail, string userId, string token);
    }
}
