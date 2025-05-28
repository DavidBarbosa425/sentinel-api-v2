using sentinel_api.Application.DTOs;
using sentinel_api.Core.Entities;

namespace sentinel_api.Core.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Succeeded, IEnumerable<string> Errors)> RegisterAsync(RegisterDto dto);
        Task GenerateEmailConfirmationTokenAsync(User user);
    }
}
