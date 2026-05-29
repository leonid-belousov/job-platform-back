using JobPlatform.Core.Entities.Users;

namespace JobPlatform.DAL.Interfaces;

public interface IPasswordService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string password);
}