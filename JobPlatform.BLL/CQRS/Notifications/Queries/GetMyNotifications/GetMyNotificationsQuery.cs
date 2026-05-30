using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Notifications.DTO;
using JobPlatform.Core.Entities.Notifications;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Notifications.Queries.GetMyNotifications;

public sealed record GetMyNotificationsQuery(bool? IsRead, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<NotificationDto>>
{
    public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, PagedResult<NotificationDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public async Task<PagedResult<NotificationDto>> Handle(GetMyNotificationsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

            var query = _db.Set<Notification>()
                .AsNoTracking()
                .Where(x => x.UserId == userId && !x.IsDeleted);

            if (request.IsRead.HasValue)
            {
                query = request.IsRead.Value
                    ? query.Where(x => x.ReadAt != null)
                    : query.Where(x => x.ReadAt == null);
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new NotificationDto(
                    x.Id,
                    x.Type,
                    x.Title,
                    x.Message,
                    x.EntityType,
                    x.EntityId,
                    x.ReadAt != null,
                    x.ReadAt,
                    x.CreatedAt))
                .ToListAsync(cancellationToken);

            return new PagedResult<NotificationDto>(items, total, request.Page, request.PageSize);
        }
    }
}