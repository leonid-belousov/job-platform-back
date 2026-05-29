namespace JobPlatform.BLL.CQRS.Auth.DTO;

public sealed record AuthResponse(string AccessToken, string RefreshToken, Guid UserId, string Email);