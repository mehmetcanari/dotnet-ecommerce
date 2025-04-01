using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineStoreWeb.API.Services.Token;

public interface ITokenService
{
    string GenerateToken(string email, IList<string> roles);
} 