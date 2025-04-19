namespace ECommerce.Domain.Model;

public class Account
{
    public int AccountId { get; init; }
    public required string Role { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Address { get; set; }
    public required string PhoneNumber { get; set; }
    public required DateTime DateOfBirth { get; init; }
    public DateTime UserCreated { get; init; } = DateTime.UtcNow;
    public DateTime UserUpdated { get; set; }
    public bool IsBanned 
    { 
        get 
        {
            if (BannedAt.HasValue && BannedUntil.HasValue)
            {
                return BannedAt.Value < DateTime.UtcNow && BannedUntil.Value > DateTime.UtcNow;
            }
            else
            {
                return false;
            }
        }
    }
    public DateTime? BannedAt { get; set; }
    public DateTime? BannedUntil { get; set; }
    public string? BanReason { get; set; }

    public void BanAccount(DateTime until, string reason)
    {
        if (IsBanned)
        {
            throw new InvalidOperationException("Account is already banned");
        }
        BannedAt = DateTime.UtcNow;
        BannedUntil = until.ToUniversalTime();
        BanReason = reason;
    }
    
    public void UnbanAccount()
    {
        if (!IsBanned)
        {
            throw new InvalidOperationException("Account is not banned");
        }
        BannedAt = null;
        BannedUntil = null;
        BanReason = null;
    }
}