using sentinel_api.Application.Common;
using sentinel_api.Application.DTOs;
using sentinel_api.Core.Entities;

namespace sentinel_api.Core.Interfaces
{
    public interface IAuthService
    {
        Task<Result> RegisterAsync(RegisterDto dto);
        Task<Result> SendEmailConfirmationAsync(User user);
        Task<Result> ConfirmUserEmailAsync(Guid id);
        Task<Result> LoginAsync(LoginDto dto);
        Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto);

    }
}
