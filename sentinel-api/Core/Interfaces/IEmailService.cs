using sentinel_api.Core.Entities;

namespace sentinel_api.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailConfirmToken emailConfirmToken);
        string GerarLinkConfirmacao(EmailConfirmToken emailConfirmToken);
    }
}
