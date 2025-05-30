using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using sentinel_api.Application.Common;
using sentinel_api.Core.Entities;
using sentinel_api.Core.Interfaces;
using sentinel_api.Infrastructure.Data;
using System.Threading.Tasks;
namespace sentinel_api.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public EmailService(
            UserManager<User> userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            AppDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }
        public async Task SendEmailConfirmationAsync(User user)
        {
            var smtpServer = _configuration["Smtp:Server"];
            var port = int.Parse(_configuration["Smtp:Port"]);
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var emailFrom = _configuration["Email:From"];
            var sender = _configuration["sender"];

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(sender, emailFrom));
            emailMessage.To.Add(new MailboxAddress("", user.Email));
            emailMessage.Subject = "Confirmação de E-mail";

            var message = await GenerateConfirmationLink(user);

            emailMessage.Body = new TextPart("html") { Text = message };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpServer, port, false);
                await client.AuthenticateAsync(username, password);
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
            var smtpServer = _configuration["Smtp:Server"];
            var port = int.Parse(_configuration["Smtp:Port"]);
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var emailFrom = _configuration["Email:From"];
            var sender = _configuration["sender"];

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(sender, emailFrom));
            emailMessage.To.Add(new MailboxAddress("", user.Email));
            emailMessage.Subject = "Reset Password";

            var message = await GeneratePasswordResetLink(user);
            emailMessage.Body = new TextPart("html") { Text = message };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpServer, port, false);
                await client.AuthenticateAsync(username, password);
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
