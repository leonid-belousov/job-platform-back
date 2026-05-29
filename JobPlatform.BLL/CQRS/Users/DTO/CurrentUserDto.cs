namespace JobPlatform.BLL.CQRS.Users.DTO;

public sealed record CurrentUserDto(Guid Id, string Email, IReadOnlyCollection<string> Roles, IReadOnlyCollection<string> Permissions);