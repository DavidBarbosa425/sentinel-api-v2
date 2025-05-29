namespace sentinel_api.Core.Entities
{
    public class EmailConfirmToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; } = DateTime.UtcNow.AddHours(1);

        public EmailConfirmToken() { }
        public EmailConfirmToken(User user, string token)
        {
            UserId = user.Id;
            Name = string.IsNullOrEmpty(user.UserName) ? "" : user.UserName;
            Email = string.IsNullOrEmpty(user.Email) ? "" : user.Email;
            Token = token;
        }
    }
}
