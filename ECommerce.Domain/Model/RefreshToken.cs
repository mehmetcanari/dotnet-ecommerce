namespace ECommerce.Domain.Model
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Expires { get; set; }
        public DateTime? Revoked { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? ReasonRevoked { get; set; }
        public string UserId { get; set; } = string.Empty;

        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsRevoked => Revoked != null;

        public void RevokeToken(string? reason = null, string? replacedByToken = null)
        {
            Revoked = DateTime.UtcNow;
            ReasonRevoked = reason;
            ReplacedByToken = replacedByToken;
        }
    }
}

