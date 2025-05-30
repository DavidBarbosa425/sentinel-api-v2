using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using sentinel_api.Application.Common;
using sentinel_api.Application.DTOs;
using sentinel_api.Core.Entities;
using sentinel_api.Core.Interfaces;
using sentinel_api.Domain.Interfaces;
using sentinel_api.Infrastructure.Data;



namespace sentinel_api.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;
        public AuthService(
            UserManager<User> userManager,
            AppDbContext context,
            IEmailService emailService,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _context = context;
            _jwtService = jwtService;
        }
        public async Task<Result> RegisterAsync(RegisterDto dto)
        {
            var user = new User(dto);
            var creationResult = await _userManager.CreateAsync(user, dto.Senha);

            if (!creationResult.Succeeded)
                return Result.Failure("Erro ao criar usuario");

            await _emailService.SendEmailConfirmationAsync(user);

            return Result.Success("E-mail de confirmação enviado com sucesso! Confira sua caixa de entrada.");
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

            await _emailService.SendEmailPasswordResetAsync(user);

            return Result.Success("E-mail de recuperar senha enviado com sucesso! Confira sua caixa de entrada.");

        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
                return Result.Failure("Usuário não encontrado.");

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

            return Result.Success("Senha redefinida com sucesso! Você pode fazer login agora.");
        }

        public async Task<Result> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Result.Failure("Usuário não encontrado ou senha inválida");

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Result.Failure("Confirme seu e-mail antes de fazer login.");

            var token = _jwtService.GenerateJwtToken(user);

            return Result<string>.Success(token, "pega o token ai");
        }

    }

}
