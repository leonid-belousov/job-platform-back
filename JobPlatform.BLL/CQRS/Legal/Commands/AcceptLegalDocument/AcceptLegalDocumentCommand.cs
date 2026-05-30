using JobPlatform.Core.Entities.Legal;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Legal.Commands.AcceptLegalDocument;

public sealed record AcceptLegalDocumentCommand(string Type, string Version, string Language, string? IpAddress) : IRequest
{
    public sealed class Handler : IRequestHandler<AcceptLegalDocumentCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task Handle(AcceptLegalDocumentCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var type = request.Type.Trim().ToLowerInvariant();
            var version = request.Version.Trim();
            var language = string.IsNullOrWhiteSpace(request.Language) ? "ru" : request.Language.Trim().ToLowerInvariant();

            var documentExists = await _db.Set<LegalDocument>().AnyAsync(x =>
                x.Type == type && x.Version == version && x.Language == language && x.IsActive && !x.IsDeleted,
                cancellationToken);

            if (!documentExists)
            {
                throw new InvalidOperationException("Active legal document version not found.");
            }

            var existing = await _db.Set<UserLegalConsent>().FirstOrDefaultAsync(x =>
                x.UserId == userId && x.DocumentType == type && x.Version == version && x.Language == language,
                cancellationToken);

            if (existing is not null)
            {
                return;
            }

            await _db.Set<UserLegalConsent>().AddAsync(new UserLegalConsent
            {
                UserId = userId,
                DocumentType = type,
                Version = version,
                Language = language,
                AcceptedAt = DateTimeOffset.UtcNow,
                IpAddress = request.IpAddress
            }, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}