using sentinel_api.Core.Entities;

namespace sentinel_api.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(User user);
        Task<string> GenerateConfirmationLink(User user);
        Task SendEmailPasswordResetAsync(User user);
    }
}
