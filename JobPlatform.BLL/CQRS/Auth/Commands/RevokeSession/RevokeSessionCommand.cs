using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Auth.Commands.RevokeSession;

public sealed record RevokeSessionCommand(Guid SessionId) : IRequest
{
    public class RevokeSessionCommandHandler : IRequestHandler<RevokeSessionCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public RevokeSessionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Пользователь не авторизован.");
            
            var session = await _db.Set<Core.Entities.Users.RefreshToken>()
                              .FirstOrDefaultAsync(x => x.Id == request.SessionId && x.UserId == userId,
                                  cancellationToken)
                          ?? throw new KeyNotFoundException("Сессия не найдена.");

            if (session.RevokedAt is null)
                session.RevokedAt = DateTimeOffset.UtcNow;
            
            await _auditService.AddAsync(new AuditEvent(
                AuditActions.AuthSessionRevoked,
                EntityType: "RefreshToken",
                EntityId: session.Id,
                NewValue: new { session.UserId },
                UserId: userId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}