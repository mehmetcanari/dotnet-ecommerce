
namespace ECommerce.Domain.Model;

public class AccessToken
{
    public required string Token { get; set; }
    public required DateTime Expires { get; set; }
}


