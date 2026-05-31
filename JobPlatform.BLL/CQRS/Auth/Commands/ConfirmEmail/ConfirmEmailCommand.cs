using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Auth;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Auth.Commands.ConfirmEmail;

public sealed record ConfirmEmailCommand(string Token) : IRequest
{
    public sealed class Handler : IRequestHandler<ConfirmEmailCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly IAuditService _auditService;

        public Handler(IApplicationDbContext db, IAuditService auditService)
        {
            _db = db;
            _auditService = auditService;
        }

        public async Task Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var tokenHash = AuthTokenHelper.HashToken(request.Token.Trim());
            var token = await _db.Set<UserAuthToken>()
                            .Include(x => x.User)
                            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash
                                                      && x.Type == UserAuthTokenTypes.EmailConfirmation
                                                      && x.UsedAt == null
                                                      && !x.IsDeleted,
                                cancellationToken)
                        ?? throw new InvalidOperationException("Token is invalid or expired.");

            if (token.ExpiresAt < DateTimeOffset.UtcNow)
            {
                throw new InvalidOperationException("Token is invalid or expired.");
            }

            token.UsedAt = DateTimeOffset.UtcNow;
            token.User.EmailConfirmed = true;
            if (token.User.Status == UserStatus.PendingConfirmation)
            {
                token.User.Status = UserStatus.Active;
            }
            token.User.UpdatedAt = DateTimeOffset.UtcNow;

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.AuthEmailConfirmed,
                EntityType: nameof(User),
                EntityId: token.UserId,
                NewValue: new { token.User.Email, token.User.EmailConfirmed, token.User.Status },
                UserId: token.UserId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
