using JobPlatform.BLL.CQRS.Users.DTO;
using JobPlatform.Core.Entities.Users;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Users.Queries.GetUsersList;

public sealed record GetUsersListQuery(string? Search, int Page = 1, int PageSize = 20)
    : IRequest<IReadOnlyCollection<UserListItemDto>>
{
    public class GetUsersListQueryHandler : IRequestHandler<GetUsersListQuery, IReadOnlyCollection<UserListItemDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetUsersListQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyCollection<UserListItemDto>> Handle(GetUsersListQuery request,
            CancellationToken cancellationToken)
        {
            var query = _db.Set<User>().AsNoTracking().Include(x => x.UserRoles).ThenInclude(x => x.Role)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLowerInvariant();

                query = query.Where(x =>
                    x.Email.ToLower().Contains(search) || (x.Phone != null && x.Phone.Contains(search)));
            }

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((Math.Max(request.Page, 1) - 1) * Math.Clamp(request.PageSize, 1, 100))
                .Take(Math.Clamp(request.PageSize, 1, 100))
                .Select(x => new UserListItemDto(x.Id, x.Email, x.Phone, x.Status.ToString(), x.EmailConfirmed,
                    x.UserRoles.Select(r => r.Role.Code).ToArray(), x.CreatedAt))
                .ToArrayAsync(cancellationToken);
        }
    }
}