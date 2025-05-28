using Microsoft.AspNetCore.Identity;
using sentinel_api.Application.DTOs;

namespace sentinel_api.Core.Entities
{
    public class User : IdentityUser
    {
        public User()
        {
            
        }
        public User(RegisterDto dto)
        {
            UserName = dto.Name;
            Email = dto.Email;
        }
    }
}
