namespace JobPlatform.BLL.CQRS.Users.DTO;

public sealed record UserListItemDto(Guid Id, string Email, string? Phone, string Status, bool EmailConfirmed, IReadOnlyCollection<string> Roles, DateTimeOffset CreatedAt);