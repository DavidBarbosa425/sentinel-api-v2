using sentinel_api.Core.Entities;

namespace sentinel_api.Core.Interfaces
{
    public interface IEmailService
    {
        Task EmailConfirmationAsync(User user);
        Task EmailPasswordResetAsync(User user);
    }
}
