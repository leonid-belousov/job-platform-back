using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Companies.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Files;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Companies.Commands.UpdateCompanyLogo;

public sealed record UpdateCompanyLogoCommand(Guid CompanyId, Guid LogoFileId) : IRequest<CompanyDto>
{
    public class UpdateCompanyLogoCommandHandler : IRequestHandler<UpdateCompanyLogoCommand, CompanyDto>
    {
        private static readonly string[] AllowedLogoContentTypes = { "image/jpeg", "image/png", "image/webp" };

        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public UpdateCompanyLogoCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<CompanyDto> Handle(UpdateCompanyLogoCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var hasAccess = await _db.Set<CompanyMember>()
                .AnyAsync(x => x.CompanyId == request.CompanyId && x.UserId == userId && x.Status == "Active",
                    cancellationToken);
            if (!hasAccess) throw new UnauthorizedAccessException("Нет доступа к компании.");

            var company = await _db.Set<Company>()
                              .FirstOrDefaultAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken)
                          ?? throw new KeyNotFoundException("Компания не найдена.");

            var file = await _db.Set<StoredFile>()
                           .FirstOrDefaultAsync(x => x.Id == request.LogoFileId && !x.IsDeleted, cancellationToken)
                       ?? throw new KeyNotFoundException("Файл не найден.");

            if (file.UploadedByUserId != userId)
                throw new UnauthorizedAccessException(
                    "Можно привязать только файл, загруженный текущим пользователем.");

            if (!AllowedLogoContentTypes.Contains(file.ContentType))
                throw new InvalidOperationException("Для логотипа допустимы только JPEG, PNG или WEBP.");

            var oldLogoFileId = company.LogoFileId;
            company.LogoFileId = file.Id;
            
            await _auditService.AddAsync(new AuditEvent(
                AuditActions.CompanyLogoUpdated,
                EntityType: nameof(Company),
                EntityId: company.Id,
                OldValue: new { LogoFileId = oldLogoFileId },
                NewValue: new { company.LogoFileId },
                UserId: userId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            return new CompanyDto(company.Id, company.Name,company.Type, company.Description, company.Industry, company.Website,
                company.LogoFileId, company.Status, company.VerifiedAt);
        }
    }
}