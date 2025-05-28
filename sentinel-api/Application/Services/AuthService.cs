using Microsoft.AspNetCore.Identity;
using MailKit.Net.Smtp;
using MimeKit;
using sentinel_api.Application.DTOs;
using sentinel_api.Core.Entities;
using sentinel_api.Core.Interfaces;
using sentinel_api.Infrastructure.Data;



namespace sentinel_api.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;


        public AuthService(
            UserManager<User> userManager, 
            AppDbContext context,
            IEmailService emailService)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
        }
        public async Task<(bool Succeeded, IEnumerable<string> Errors)> RegisterAsync(RegisterDto dto)
        {
            var user = new User(dto);

            var result = await _userManager.CreateAsync(user, dto.Senha);

            if (!result.Succeeded)
                return (false, result.Errors.Select(e => e.Description));

            await GenerateEmailConfirmationTokenAsync(user);

            return (true, null);
        }

        public async Task GenerateEmailConfirmationTokenAsync(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var tokenEmailConfirmation = new EmailConfirmToken(user, token);

            _context.EmailConfirmTokens.Add(tokenEmailConfirmation);

            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(tokenEmailConfirmation);
        }

    }
}
