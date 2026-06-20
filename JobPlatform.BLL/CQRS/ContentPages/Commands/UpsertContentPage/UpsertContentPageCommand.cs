using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.ContentPages.Builders;
using JobPlatform.BLL.CQRS.ContentPages.DTO;
using JobPlatform.Core.Entities.Content;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.ContentPages.Commands.UpsertContentPage;

public sealed record UpsertContentPageCommand(
    string Slug,
    int SchemaVersion,
    bool IsPublished,
    IReadOnlyList<ContentPageBlockDto> Blocks) : IRequest<ContentPageDto>
{
    public sealed class Handler : IRequestHandler<UpsertContentPageCommand, ContentPageDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser, IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<ContentPageDto> Handle(UpsertContentPageCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var slug = request.Slug.Trim().ToLowerInvariant();

            if (request.SchemaVersion < 1)
            {
                throw new InvalidOperationException("Content page schema version must be greater than zero.");
            }

            if (request.Blocks.Count == 0)
            {
                throw new InvalidOperationException("Content page must contain at least one block.");
            }

            var jsonContent = ContentPageBuilder.ToJson(request.SchemaVersion, request.Blocks);

            var page = await _db.Set<ContentPage>()
                .FirstOrDefaultAsync(x => x.Slug == slug && !x.IsDeleted, cancellationToken);

            object? oldValue = null;

            if (page is null)
            {
                page = new ContentPage
                {
                    Slug = slug
                };

                await _db.Set<ContentPage>().AddAsync(page, cancellationToken);
            }
            else
            {
                oldValue = new
                {
                    page.SchemaVersion,
                    page.IsPublished,
                    page.PublishedAt,
                    page.JsonContent
                };
            }

            page.SchemaVersion = request.SchemaVersion;
            page.JsonContent = jsonContent;
            page.IsPublished = request.IsPublished;
            page.PublishedAt = request.IsPublished ? DateTimeOffset.UtcNow : null;
            page.UpdatedByUserId = userId;

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.ContentPageUpserted,
                EntityType: nameof(ContentPage),
                EntityId: page.Id,
                OldValue: oldValue,
                NewValue: new
                {
                    page.Slug,
                    page.SchemaVersion,
                    page.IsPublished,
                    page.PublishedAt,
                    page.JsonContent
                },
                UserId: userId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            return ContentPageBuilder.FromEntity(page);
        }
    }
}
