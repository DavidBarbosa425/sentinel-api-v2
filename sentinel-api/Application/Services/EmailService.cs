
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MimeKit;
using sentinel_api.Core.Entities;
using sentinel_api.Core.Interfaces;
using sentinel_api.Domain.Constants;
using sentinel_api.Infrastructure.Configurations;
using sentinel_api.Infrastructure.Data;

namespace sentinel_api.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly SmtpSettings _smtpSettings;
        private readonly EmailSettings _emailSettings;

        public EmailService(    
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor,
            AppDbContext context,
            IOptions<SmtpSettings> smtpOptions,
            IOptions<EmailSettings> emailOptions)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _smtpSettings = smtpOptions.Value;
            _emailSettings = emailOptions.Value;
        }
        private async Task SendEmailAsync(MimeMessage emailMessage)
        {
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, false);
                await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }

        private MimeMessage CreateEmail(User user, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailSettings.Sender, _emailSettings.From));
            emailMessage.To.Add(new MailboxAddress("", user.Email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html") { Text = message };

            return emailMessage;
        }

        public async Task EmailConfirmationAsync(User user)
        {
            var emailMessage = CreateEmail(user, EmailSubjects.Confirmation, await GenerateMessage(user, EmailSubjects.Confirmation));
            await SendEmailAsync(emailMessage);
        }

        public async Task EmailPasswordResetAsync(User user)
        {
            var emailMessage = CreateEmail(user, EmailSubjects.PasswordReset, await GenerateMessage(user, EmailSubjects.PasswordReset));
            await SendEmailAsync(emailMessage);
        }

        private async Task<string> GenerateMessage(User user, string subject)
        {

            var request = _httpContextAccessor.HttpContext?.Request;
            var scheme = request?.Scheme ?? "https";
            var host = request?.Host.ToString() ?? "localhost";

            var htmlMessage = string.Empty;

            if (subject == EmailSubjects.Confirmation)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var emailConfirmToken = new EmailConfirmToken(user, token);
                await SaveEmailTokenAsync(emailConfirmToken);

                var confirmationLink = $"{scheme}://{host}/api/auth/confirmUserEmail?id={emailConfirmToken.Id}";

                htmlMessage = $@"
                <p>Olá {emailConfirmToken.Name},</p>
                <p>Clique no botão abaixo para confirmar seu e-mail:</p>
                <p><a style='padding: 10px 20px; background-color: #4CAF50; color: white; text-decoration: none;' href='{confirmationLink}'>Confirmar E-mail</a></p>
                <p>Se você não se registrou, ignore este e-mail.</p>
                 ";

                return htmlMessage;
            }

            if (subject == EmailSubjects.PasswordReset)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var emailConfirmToken = new EmailConfirmToken(user, token);

                await SaveEmailTokenAsync(emailConfirmToken);

                var confirmationLink = $"{scheme}://{host}/api/auth/resetPassword?email={user.Email}&token={Uri.EscapeDataString(emailConfirmToken.Token)}";

                htmlMessage = $@"
                <p>Olá {emailConfirmToken.Name},</p>
                <p>Clique no botão abaixo para redefinir sua senha:</p>
                <p><a style='padding: 10px 20px; background-color: #4CAF50; color: white; text-decoration: none;' href='{confirmationLink}'>Redefinir Senha</a></p>
                <p>Se você não pediu para redefinir sua senha, ignore este e-mail.</p>
             ";

                return htmlMessage;
            }

            return htmlMessage;
        }

        private async Task SaveEmailTokenAsync(EmailConfirmToken emailToken)
        {
            _context.EmailConfirmTokens.Add(emailToken);
            await _context.SaveChangesAsync();
        }

    }
}
