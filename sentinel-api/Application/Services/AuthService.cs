using Microsoft.AspNetCore.Identity;
using MailKit.Net.Smtp;
using MimeKit;
using sentinel_api.Application.DTOs;
using sentinel_api.Core.Entities;
using sentinel_api.Core.Interfaces;
using sentinel_api.Infrastructure.Data;
using sentinel_api.Application.Common;



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
        public async Task<Result> RegisterAsync(RegisterDto dto)
        {
            var user = new User(dto);
            var creationResult = await _userManager.CreateAsync(user, dto.Senha);

            if (!creationResult.Succeeded)
                return Result.Failure("Erro ao criar usuario");

            await SendEmailConfirmationAsync(user);

            return Result.Success("Usuário Registrado com Sucesso!");
        }

        public async Task SendEmailConfirmationAsync(User user)
        {
            var token = await GenerateEmailTokenAsync(user);

            var emailToken = new EmailConfirmToken(user, token);

            await SaveEmailTokenAsync(emailToken);

            await _emailService.SendEmailAsync(emailToken);
        }

        private async Task<string> GenerateEmailTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        private async Task SaveEmailTokenAsync(EmailConfirmToken emailToken)
        {
            _context.EmailConfirmTokens.Add(emailToken);
            await _context.SaveChangesAsync();
        }

    }
}
