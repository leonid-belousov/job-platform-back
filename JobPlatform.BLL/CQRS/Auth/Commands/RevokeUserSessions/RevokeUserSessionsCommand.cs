using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Auth.Commands.RevokeUserSessions;

public sealed record RevokeUserSessionsCommand(Guid UserId) : IRequest
{
    public class RevokeUserSessionsCommandHandler : IRequestHandler<RevokeUserSessionsCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly IAuditService _auditService;
        private readonly ICurrentUserService _currentUser;

        public RevokeUserSessionsCommandHandler(IApplicationDbContext db, IAuditService auditService, ICurrentUserService currentUser)
        {
            _db = db;
            _auditService = auditService;
            _currentUser = currentUser;
        }

        public async Task Handle(RevokeUserSessionsCommand request, CancellationToken cancellationToken)
        {
            var userExists = await _db.Set<User>().AnyAsync(x => x.Id == request.UserId, cancellationToken);
            if (!userExists)
                throw new KeyNotFoundException("Пользователь не найден.");

            var now = DateTimeOffset.UtcNow;
            var sessions = await _db.Set<Core.Entities.Users.RefreshToken>()
                .Where(x => x.UserId == request.UserId && x.RevokedAt == null && x.ExpiresAt > now)
                .ToListAsync(cancellationToken);

            foreach (var session in sessions)
                session.RevokedAt = now;
            
            await _auditService.AddAsync(new AuditEvent(
                AuditActions.AuthUserSessionsRevokedByAdmin,
                EntityType: "User",
                EntityId: request.UserId,
                NewValue: new { UserId = request.UserId, RevokedCount = sessions.Count },
                UserId: _currentUser.UserId), cancellationToken);
            
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}