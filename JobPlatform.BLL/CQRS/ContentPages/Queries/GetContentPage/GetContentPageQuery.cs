using JobPlatform.BLL.CQRS.ContentPages.Builders;
using JobPlatform.BLL.CQRS.ContentPages.DTO;
using JobPlatform.Core.Entities.Content;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.ContentPages.Queries.GetContentPage;

public sealed record GetContentPageQuery(string Slug, bool IncludeDraft = false) : IRequest<ContentPageDto?>
{
    public sealed class Handler : IRequestHandler<GetContentPageQuery, ContentPageDto?>
    {
        private readonly IApplicationDbContext _db;

        public Handler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ContentPageDto?> Handle(GetContentPageQuery request, CancellationToken cancellationToken)
        {
            var slug = request.Slug.Trim().ToLowerInvariant();

            var page = await _db.Set<ContentPage>()
                .AsNoTracking()
                .Where(x => x.Slug == slug && !x.IsDeleted)
                .Where(x => request.IncludeDraft || x.IsPublished)
                .FirstOrDefaultAsync(cancellationToken);

            if (page is not null)
            {
                return ContentPageBuilder.FromEntity(page);
            }

            return slug == "about" ? ContentPageBuilder.BuildDefaultAboutPage() : null;
        }
    }
}
