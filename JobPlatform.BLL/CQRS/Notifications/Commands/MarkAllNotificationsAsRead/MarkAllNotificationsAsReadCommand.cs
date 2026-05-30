using JobPlatform.Core.Entities.Notifications;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Notifications.Commands.MarkAllNotificationsAsRead;

public sealed record MarkAllNotificationsAsReadCommand : IRequest<int>
{
    public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, int>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public MarkAllNotificationsAsReadCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<int> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var now = DateTimeOffset.UtcNow;
            var unread = await _db.Set<Notification>()
                .Where(x => x.UserId == userId && x.ReadAt == null && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var notification in unread)
            {
                notification.ReadAt = now;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return unread.Count;
        }
    }
}