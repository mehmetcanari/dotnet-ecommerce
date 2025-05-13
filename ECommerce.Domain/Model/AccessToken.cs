
namespace ECommerce.Domain.Model;

public class AccessToken
{
    public required string Token { get; init; }
    public required DateTime Expires { get; init; }
}


