using JobPlatform.Core.Entities.Notifications;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Notifications.Commands.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand(Guid NotificationId) : IRequest
{
    public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public MarkNotificationAsReadCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var notification = await _db.Set<Notification>()
                                   .FirstOrDefaultAsync(
                                       x => x.Id == request.NotificationId && x.UserId == userId && !x.IsDeleted,
                                       cancellationToken)
                               ?? throw new KeyNotFoundException("Уведомление не найдено.");

            notification.ReadAt ??= DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}