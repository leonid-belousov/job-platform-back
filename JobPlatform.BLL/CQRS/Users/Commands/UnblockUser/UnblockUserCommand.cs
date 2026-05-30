using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Users.Commands.UnblockUser;

public sealed record UnblockUserCommand(Guid UserId) : IRequest
{
    public class UnblockUserCommandHandler : IRequestHandler<UnblockUserCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;
        public UnblockUserCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task Handle(UnblockUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Set<User>()
                           .FirstOrDefaultAsync(x => x.Id == request.UserId && !x.IsDeleted, cancellationToken)
                       ?? throw new InvalidOperationException("Пользователь не найден.");

            var oldStatus = user.Status;
            user.Status = UserStatus.Active;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            
            await _auditService.AddAsync(new AuditEvent(
                AuditActions.UserUnblocked,
                EntityType: nameof(User),
                EntityId: user.Id,
                OldValue: new { Status = oldStatus.ToString() },
                NewValue: new { Status = user.Status.ToString() },
                UserId: _currentUser.UserId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}