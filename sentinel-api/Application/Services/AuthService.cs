using Microsoft.AspNetCore.Identity;
using MailKit.Net.Smtp;
using MimeKit;
using sentinel_api.Application.DTOs;
using sentinel_api.Core.Entities;
using sentinel_api.Core.Interfaces;
using sentinel_api.Infrastructure.Data;
using sentinel_api.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;



namespace sentinel_api.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        public AuthService(
            UserManager<User> userManager,
            AppDbContext context,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }
        public async Task<Result> RegisterAsync(RegisterDto dto)
        {
            var user = new User(dto);
            var creationResult = await _userManager.CreateAsync(user, dto.Senha);

            if (!creationResult.Succeeded)
                return Result.Failure("Erro ao criar usuario");

            return await SendEmailConfirmationAsync(user);
        }
        public async Task<Result> SendEmailConfirmationAsync(User user)
        {
            var token = await GenerateEmailTokenAsync(user);

            if (string.IsNullOrEmpty(token))
                return Result.Failure("Erro ao gerar token de confirmação de e-mail.");

            var emailToken = new EmailConfirmToken(user, token);

            await SaveEmailTokenAsync(emailToken);

            await _emailService.SendEmailAsync(emailToken);

            return Result.Success("E-mail de confirmação enviado com sucesso! Confira sua caixa de entrada.");
        }

        private async Task<string> GenerateEmailTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        private async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        private async Task SaveEmailTokenAsync(EmailConfirmToken emailToken)
        {
            _context.EmailConfirmTokens.Add(emailToken);
            await _context.SaveChangesAsync();
        }

        public async Task<Result> ConfirmUserEmailAsync(Guid id)
        {
            var tokenEntry = await _context.EmailConfirmTokens
                .FirstOrDefaultAsync(t => t.Id == id && t.Expiration > DateTime.UtcNow);

            if (tokenEntry == null) return Result.Failure("Token de confirmação inválido ou expirado.");

            var user = await _userManager.FindByIdAsync(tokenEntry.UserId);

            if (user == null) return Result.Failure("Usuário não encontrado.");

            var result = await _userManager.ConfirmEmailAsync(user, tokenEntry.Token);

            if (!result.Succeeded) return Result.Failure("Erro ao confirmar o e-mail do usuário.");

            _context.EmailConfirmTokens.Remove(tokenEntry);
            await _context.SaveChangesAsync();

            return Result.Success("E-mail confirmado com sucesso!");

        }

        public async Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
                return Result.Failure("Se o e-mail estiver cadastrado, enviaremos um link de redefinição.");

            var token = await GeneratePasswordResetTokenAsync(user);

            if (string.IsNullOrEmpty(token))
                return Result.Failure("Erro ao gerar token de confirmação de e-mail.");

            var emailToken = new EmailConfirmToken(user, token);

            await SaveEmailTokenAsync(emailToken);

            await _emailService.SendEmailPasswordResetAsync(emailToken,user);

            return Result.Success("E-mail de confirmação enviado com sucesso! Confira sua caixa de entrada.");

        }

        public async Task<Result> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Result.Failure("Usuário não encontrado ou senha inválida");

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Result.Failure("Confirme seu e-mail antes de fazer login.");

            var token = GenerateJwtToken(user);

            return Result<string>.Success(token, "pega o token ai");
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }

}
