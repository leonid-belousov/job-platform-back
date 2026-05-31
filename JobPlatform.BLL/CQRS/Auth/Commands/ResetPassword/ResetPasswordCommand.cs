using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Auth;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(string Token, string NewPassword) : IRequest
{
    public sealed class Handler : IRequestHandler<ResetPasswordCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly IPasswordService _passwordService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IAuditService _auditService;

        public Handler(IApplicationDbContext db, IPasswordService passwordService,
            IJwtTokenService jwtTokenService, IAuditService auditService)
        {
            _db = db;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
            _auditService = auditService;
        }

        public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var tokenHash = AuthTokenHelper.HashToken(request.Token.Trim());
            var token = await _db.Set<UserAuthToken>()
                            .Include(x => x.User)
                            .ThenInclude(x => x.RefreshTokens)
                            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash
                                                      && x.Type == UserAuthTokenTypes.PasswordReset
                                                      && x.UsedAt == null
                                                      && !x.IsDeleted,
                                cancellationToken)
                        ?? throw new InvalidOperationException("Token is invalid or expired.");

            if (token.ExpiresAt < DateTimeOffset.UtcNow || token.User.Status == UserStatus.Blocked)
            {
                throw new InvalidOperationException("Token is invalid or expired.");
            }

            token.UsedAt = DateTimeOffset.UtcNow;
            token.User.PasswordHash = _passwordService.HashPassword(token.User, request.NewPassword);
            token.User.UpdatedAt = DateTimeOffset.UtcNow;

            foreach (var refreshToken in token.User.RefreshTokens.Where(x => x.RevokedAt == null))
            {
                refreshToken.RevokedAt = DateTimeOffset.UtcNow;
                refreshToken.ReplacedByTokenHash = null;
            }

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.AuthPasswordResetCompleted,
                EntityType: nameof(User),
                EntityId: token.UserId,
                NewValue: new { token.User.Email },
                UserId: token.UserId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
