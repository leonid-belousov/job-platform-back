using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Users.Commands.BlockUser;

public sealed record BlockUserCommand(Guid UserId) : IRequest
{
    public class BlockUserCommandHandler : IRequestHandler<BlockUserCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public BlockUserCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task Handle(BlockUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Set<User>()
                           .FirstOrDefaultAsync(x => x.Id == request.UserId && !x.IsDeleted, cancellationToken)
                       ?? throw new InvalidOperationException("Пользователь не найден.");

            var oldStatus = user.Status;
            user.Status = UserStatus.Blocked;
            user.UpdatedAt = DateTimeOffset.UtcNow;
            await _auditService.AddAsync(new AuditEvent(
                AuditActions.UserBlocked,
                EntityType: nameof(User),
                EntityId: user.Id,
                OldValue: new { Status = oldStatus.ToString() },
                NewValue: new { Status = user.Status.ToString() },
                UserId: _currentUser.UserId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}