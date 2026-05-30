using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Companies.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Moderation;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Companies.Commands.UpdateCompany;

public sealed record UpdateCompanyCommand(
    Guid CompanyId,
    string Name,
    string? Description,
    string? Industry,
    string? Website) : IRequest<CompanyDto>
{
    public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public UpdateCompanyCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            
            var hasAccess = await _db.Set<CompanyMember>().AnyAsync(
                x => x.CompanyId == request.CompanyId && x.UserId == userId && x.Status == "Active", cancellationToken);
            
            if (!hasAccess) throw new UnauthorizedAccessException("Нет доступа к компании.");

            var company =
                await _db.Set<Company>().FirstOrDefaultAsync(x => x.Id == request.CompanyId && !x.IsDeleted,
                    cancellationToken)
                ?? throw new InvalidOperationException("Компания не найдена.");
            
            var oldValue = new { company.Name, company.Description, company.Industry, company.Website };
            
            company.Name = request.Name.Trim();
            company.Description = request.Description;
            company.Industry = request.Industry;
            company.Website = request.Website;
            if (company.ModerationStatus == ModerationStatuses.Approved)
            {
                company.Status = "PendingVerification";
                company.ModerationStatus = ModerationStatuses.Pending;
                company.ModerationComment = null;
                company.ModeratedAt = null;
                company.ModeratedByUserId = null;
            }
            company.UpdatedAt = DateTimeOffset.UtcNow;

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.CompanyUpdated,
                EntityType: nameof(Company),
                EntityId: company.Id,
                OldValue: oldValue,
                NewValue: new { company.Name, company.Description, company.Industry, company.Website, company.Status, company.ModerationStatus },
                UserId: userId), cancellationToken);
            
            await _db.SaveChangesAsync(cancellationToken);
            return new CompanyDto(company.Id, company.Name, company.Description, company.Industry, company.Website, company.LogoFileId, company.Status, company.VerifiedAt);
        }
    }
}