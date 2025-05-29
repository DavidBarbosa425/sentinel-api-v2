using MailKit.Net.Smtp;
using MimeKit;
using sentinel_api.Core.Entities;
using sentinel_api.Core.Interfaces;
namespace sentinel_api.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public EmailService(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task SendEmailAsync(EmailConfirmToken emailConfirmToken)
        {
            var smtpServer = _configuration["Smtp:Server"];
            var port = int.Parse(_configuration["Smtp:Port"]);
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var emailFrom = _configuration["Email:From"];
            var sender = _configuration["sender"];

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(sender, emailFrom));
            emailMessage.To.Add(new MailboxAddress("", emailConfirmToken.Email));
            emailMessage.Subject = "Confirmação de E-mail";

            var message = GenerateConfirmationLink(emailConfirmToken);
            emailMessage.Body = new TextPart("html") { Text = message };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpServer, port, false);
                await client.AuthenticateAsync(username, password);
                var teste = await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }

        public string GenerateConfirmationLink(EmailConfirmToken emailConfirmToken)
        {

            var request = _httpContextAccessor.HttpContext?.Request;
            var scheme = request?.Scheme ?? "https";
            var host = request?.Host.ToString() ?? "localhost";

            var confirmationLink = $"{scheme}://{host}/api/auth/confirm-email?id={emailConfirmToken.Id}";

            var htmlMessage = $@"
            <p>Olá {emailConfirmToken.Name},</p>
            <p>Clique no botão abaixo para confirmar seu e-mail:</p>
            <p><a style='padding: 10px 20px; background-color: #4CAF50; color: white; text-decoration: none;' href='{confirmationLink}'>Confirmar E-mail</a></p>
            <p>Se você não se registrou, ignore este e-mail.</p>
             ";

            return htmlMessage;
        }
    }
}
