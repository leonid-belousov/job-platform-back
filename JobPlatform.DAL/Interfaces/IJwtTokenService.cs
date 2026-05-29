using JobPlatform.Core.Entities.Users;

namespace JobPlatform.DAL.Interfaces;

public interface IJwtTokenService
{
    string CreateAccessToken(User user, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permissions);
    string CreateRefreshToken();
    string HashRefreshToken(string refreshToken);
}