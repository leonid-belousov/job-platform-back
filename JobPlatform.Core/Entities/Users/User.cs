using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Users;

public sealed class User : BaseEntity
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.PendingConfirmation;
    public bool EmailConfirmed { get; set; }
    public bool PhoneConfirmed { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}