using JobPlatform.BLL.CQRS.Legal.DTO;
using JobPlatform.Core.Entities.Legal;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Legal.Queries.GetActiveLegalDocument;

public sealed record GetActiveLegalDocumentQuery(string Type, string Language = "ru") : IRequest<LegalDocumentDto>
{
    public sealed class Handler : IRequestHandler<GetActiveLegalDocumentQuery, LegalDocumentDto>
    {
        private readonly IApplicationDbContext _db;

        public Handler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<LegalDocumentDto> Handle(GetActiveLegalDocumentQuery request, CancellationToken cancellationToken)
        {
            var type = request.Type.Trim().ToLowerInvariant();
            var language = string.IsNullOrWhiteSpace(request.Language) ? "ru" : request.Language.Trim().ToLowerInvariant();

            if (!LegalDocumentTypes.All.Contains(type, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Legal document type is not supported.");
            }

            var document = await _db.Set<LegalDocument>()
                               .AsNoTracking()
                               .Where(x => x.Type == type && x.Language == language && x.IsActive && !x.IsDeleted)
                               .OrderByDescending(x => x.PublishedAt)
                               .FirstOrDefaultAsync(cancellationToken)
                           ?? throw new InvalidOperationException("Active legal document not found.");

            return new LegalDocumentDto(document.Id, document.Type, document.Version, document.Language,
                document.Title, document.Content, document.IsActive, document.PublishedAt);
        }
    }
}