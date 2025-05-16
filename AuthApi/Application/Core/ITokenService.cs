using AuthApi.Entities;

namespace AuthApi.Application.Core;

public interface ITokenService
{
    string CreateToken(AppUser user);
    string GenerateRefreshToken();
}