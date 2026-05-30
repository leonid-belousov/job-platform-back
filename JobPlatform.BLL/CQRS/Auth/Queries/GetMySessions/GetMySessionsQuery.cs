using JobPlatform.BLL.CQRS.Auth.DTO;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Auth.Queries.GetMySessions;

public sealed record GetMySessionsQuery : IRequest<IReadOnlyCollection<SessionDto>>
{
    public class GetMySessionsQueryHandler : IRequestHandler<GetMySessionsQuery, IReadOnlyCollection<SessionDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public GetMySessionsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyCollection<SessionDto>> Handle(GetMySessionsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Пользователь не авторизован.");
            var now = DateTimeOffset.UtcNow;

            return await _db.Set<RefreshToken>()
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new SessionDto(
                    x.Id,
                    x.CreatedAt,
                    x.ExpiresAt,
                    x.RevokedAt,
                    x.RevokedAt == null && now < x.ExpiresAt,
                    x.CreatedByIp))
                .ToListAsync(cancellationToken);
        }
    }
}