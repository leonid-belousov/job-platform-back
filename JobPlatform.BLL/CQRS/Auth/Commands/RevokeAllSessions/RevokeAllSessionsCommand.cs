using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Auth.Commands.RevokeAllSessions;

public sealed record RevokeAllSessionsCommand : IRequest
{
    public class RevokeAllSessionsCommandHandler : IRequestHandler<RevokeAllSessionsCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public RevokeAllSessionsCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task Handle(RevokeAllSessionsCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Пользователь не авторизован.");
            var now = DateTimeOffset.UtcNow;

            var sessions = await _db.Set<Core.Entities.Users.RefreshToken>()
                .Where(x => x.UserId == userId && x.RevokedAt == null && x.ExpiresAt > now)
                .ToListAsync(cancellationToken);

            foreach (var session in sessions)
                session.RevokedAt = now;

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.AuthAllSessionsRevoked,
                EntityType: "RefreshToken",
                NewValue: new { UserId = userId, RevokedCount = sessions.Count },
                UserId: userId), cancellationToken);
            
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}