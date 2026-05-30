using JobPlatform.BLL.CQRS.Dictionaries.DTO;
using JobPlatform.Core.Entities.Dictionaries;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Dictionaries.Queries.GetDictionaryItems;

public sealed record GetDictionaryItemsQuery(string Type, bool ActiveOnly = true, string? Search = null)
    : IRequest<IReadOnlyCollection<DictionaryItemDto>>
{
    public class
        GetDictionaryItemsQueryHandler : IRequestHandler<GetDictionaryItemsQuery,
        IReadOnlyCollection<DictionaryItemDto>>
    {
        private readonly IApplicationDbContext _db;

        public GetDictionaryItemsQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyCollection<DictionaryItemDto>> Handle(GetDictionaryItemsQuery request,
            CancellationToken cancellationToken)
        {
            var query = _db.Set<DictionaryItem>().AsNoTracking().Where(x => x.Type == request.Type);

            if (request.ActiveOnly)
            {
                query = query.Where(x => x.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.Trim().ToLower();
                query = query.Where(x => x.Code.ToLower().Contains(search) || x.Name.ToLower().Contains(search));
            }

            return await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x =>
                    new DictionaryItemDto(x.Id, x.Type, x.Code, x.Name, x.Description, x.SortOrder, x.IsActive))
                .ToArrayAsync(cancellationToken);
        }
    }
}