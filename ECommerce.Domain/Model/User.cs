using ECommerce.Shared.Constants;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Domain.Model;

public class User : IdentityUser<Guid>
{
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string IdentityNumber { get; init; }
    public required string City { get; set; }
    public required string Country { get; set; }
    public required string ZipCode { get; set; }
    public required string Address { get; set; }
    public required string PhoneCode { get; set; }
    public required bool MembershipAgreement { get; init; } 
    public bool PrivacyPolicyConsent { get; set; }
    public bool ElectronicConsent { get; set; }
    public required DateTime DateOfBirth { get; init; }
    public DateTime CreatedOn { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
    public bool IsBanned
    {
        get
        {
            if (BannedAt.HasValue && BannedUntil.HasValue)
            {
                return BannedAt.Value < DateTime.UtcNow && BannedUntil.Value > DateTime.UtcNow;
            }

            return false;
        }
    }
    public DateTime? BannedAt { get; private set; }
    public DateTime? BannedUntil { get; private set; }
    public string? BanReason { get; private set; }

    public void BanAccount(DateTime until, string reason)
    {
        if (IsBanned)
        {
            throw new InvalidOperationException(ErrorMessages.AccountAlreadyBanned);
        }
        BannedAt = DateTime.UtcNow;
        BannedUntil = until.ToUniversalTime();
        BanReason = reason;
    }

    public void UnbanAccount()
    {
        if (!IsBanned)
        {
            throw new InvalidOperationException(ErrorMessages.AccountAlreadyUnbanned);
        }
        BannedAt = null;
        BannedUntil = null;
        BanReason = null;
    }
}