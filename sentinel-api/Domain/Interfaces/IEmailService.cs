using sentinel_api.Core.Entities;

namespace sentinel_api.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailConfirmToken emailConfirmToken);
        string GenerateConfirmationLink(EmailConfirmToken emailConfirmToken);
        Task SendEmailPasswordResetAsync(EmailConfirmToken emailConfirmToken, User user);
    }
}
