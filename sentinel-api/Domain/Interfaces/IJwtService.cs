using sentinel_api.Core.Entities;

namespace sentinel_api.Domain.Interfaces
{
    public interface IJwtService
    {
        string GenerateJwtToken(User user);
    }
}
