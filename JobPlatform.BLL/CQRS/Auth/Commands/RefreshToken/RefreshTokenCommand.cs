using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Auth.DTO;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken, string? IpAddress) : IRequest<AuthResponse>
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
    {
        private readonly IApplicationDbContext _db;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IAuditService _auditService;
        public RefreshTokenCommandHandler(IApplicationDbContext db, IJwtTokenService jwtTokenService, IAuditService auditService)
        {
            _db = db;
            _jwtTokenService = jwtTokenService;
            _auditService = auditService;
        }

        public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken);

            var refreshToken = await _db.Set<Core.Entities.Users.RefreshToken>()
                                   .Include(x => x.User)
                                   .ThenInclude(x => x.UserRoles)
                                   .ThenInclude(x => x.Role)
                                   .ThenInclude(x => x.RolePermissions)
                                   .ThenInclude(x => x.Permission)
                                   .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken)
                               ?? throw new UnauthorizedAccessException("Refresh token недействителен.");

            if (!refreshToken.IsActive)
                throw new UnauthorizedAccessException("Refresh token истек или был отозван.");

            var user = refreshToken.User;
            if (user.Status == UserStatus.Blocked)
                throw new UnauthorizedAccessException("Пользователь заблокирован.");

            var newRefreshToken = _jwtTokenService.CreateRefreshToken();
            var newRefreshTokenHash = _jwtTokenService.HashRefreshToken(newRefreshToken);
            var now = DateTimeOffset.UtcNow;

            refreshToken.RevokedAt = now;
            refreshToken.ReplacedByTokenHash = newRefreshTokenHash;

            user.RefreshTokens.Add(new Core.Entities.Users.RefreshToken()
            {
                UserId = user.Id,
                TokenHash = newRefreshTokenHash,
                ExpiresAt = now.AddDays(30),
                CreatedByIp = request.IpAddress
            });
            
            await _auditService.AddAsync(new AuditEvent(
                AuditActions.AuthTokenRefreshed,
                EntityType: nameof(RefreshToken),
                EntityId: refreshToken.Id,
                NewValue: new { UserId = user.Id, OldRefreshTokenId = refreshToken.Id },
                UserId: user.Id), cancellationToken);
            
            await _db.SaveChangesAsync(cancellationToken);

            var roles = user.UserRoles.Select(x => x.Role.Code).ToArray();
            var permissions = user.UserRoles
                .SelectMany(x => x.Role.RolePermissions)
                .Select(x => x.Permission.Code)
                .Distinct()
                .ToArray();

            var accessToken = _jwtTokenService.CreateAccessToken(user, roles, permissions);
            return new AuthResponse(accessToken, newRefreshToken, user.Id, user.Email);
        }
    }
}