namespace ECommerce.Domain.Model;

public class RefreshToken : BaseEntity
{
    public required string Token { get; init; } = string.Empty;
    public byte[] Salt { get; init; } = [];
    public DateTime ExpiresAt { get; init; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }
    public string Email { get; init; } = string.Empty;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;

    public void RevokeToken(string? reason = null, string? replacedByToken = null)
    {
        RevokedAt = DateTime.UtcNow;
        ReasonRevoked = reason;
        ReplacedByToken = replacedByToken;
    }
}