namespace ECommerce.Domain.Model;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }
    public string Email { get; set; } = string.Empty;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;

    public void RevokeToken(string? reason = null, string? replacedByToken = null)
    {
        RevokedAt = DateTime.UtcNow;
        ReasonRevoked = reason;
        ReplacedByToken = replacedByToken;
    }
}