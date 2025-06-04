using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MimeKit;
using sentinel_api.Core.Entities;
using sentinel_api.Core.Interfaces;
using sentinel_api.Infrastructure.Configurations;
using sentinel_api.Infrastructure.Data;
namespace sentinel_api.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly SmtpSettings _smtpSettings;
        private readonly EmailSettings _emailSettings;

        public EmailService(    
            UserManager<User> userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            AppDbContext context,
            IOptions<SmtpSettings> smtpOptions,
            IOptions<EmailSettings> emailOptions)
        {
            _userManager = userManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _smtpSettings = smtpOptions.Value;
            _emailSettings = emailOptions.Value;
        }
        public async Task SendEmailConfirmationAsync(User user)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailSettings.Sender, _emailSettings.From));
            emailMessage.To.Add(new MailboxAddress("", user.Email));
            emailMessage.Subject = "Confirmação de E-mail";

            var message = await GenerateConfirmationLink(user);

            emailMessage.Body = new TextPart("html") { Text = message };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, false);
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                var teste = await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }

        private async Task SaveEmailTokenAsync(EmailConfirmToken emailToken)
        {
            _context.EmailConfirmTokens.Add(emailToken);
            await _context.SaveChangesAsync();
        }

        public async Task<string> GenerateConfirmationLink(User user)
        {

            var request = _httpContextAccessor.HttpContext?.Request;
            var scheme = request?.Scheme ?? "https";
            var host = request?.Host.ToString() ?? "localhost";

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var emailConfirmToken = new EmailConfirmToken(user, token);
            await SaveEmailTokenAsync(emailConfirmToken);

            var confirmationLink = $"{scheme}://{host}/api/auth/confirmUserEmail?id={emailConfirmToken.Id}";

            var htmlMessage = $@"
            <p>Olá {emailConfirmToken.Name},</p>
            <p>Clique no botão abaixo para confirmar seu e-mail:</p>
            <p><a style='padding: 10px 20px; background-color: #4CAF50; color: white; text-decoration: none;' href='{confirmationLink}'>Confirmar E-mail</a></p>
            <p>Se você não se registrou, ignore este e-mail.</p>
             ";

            return htmlMessage;
        }
        public async Task SendEmailPasswordResetAsync(User user)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailSettings.Sender, _emailSettings.From));
            emailMessage.To.Add(new MailboxAddress("", user.Email));
            emailMessage.Subject = "Reset Password";

            var message = await GeneratePasswordResetLink(user);
            emailMessage.Body = new TextPart("html") { Text = message };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, false);
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                var teste = await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }

        private async Task<string> GeneratePasswordResetLink(User user)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var scheme = request?.Scheme ?? "https";
            var host = request?.Host.ToString() ?? "localhost";

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var emailConfirmToken = new EmailConfirmToken(user, token);

            await SaveEmailTokenAsync(emailConfirmToken);

            var confirmationLink = $"{scheme}://{host}/api/auth/resetPassword?email={user.Email}&token={Uri.EscapeDataString(emailConfirmToken.Token)}";

            var htmlMessage = $@"
            <p>Olá {emailConfirmToken.Name},</p>
            <p>Clique no botão abaixo para confirmar seu e-mail:</p>
            <p><a style='padding: 10px 20px; background-color: #4CAF50; color: white; text-decoration: none;' href='{confirmationLink}'>Redefinir Senha</a></p>
            <p>Se você não se registrou, ignore este e-mail.</p>
             ";

            return htmlMessage;
        }
    }
}
