using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Companies.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Moderation;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Moderation.Commands.ApproveCompany;

public sealed record ApproveCompanyCommand(Guid CompanyId, string? Comment) : IRequest<CompanyDto>
{
    public class ApproveCompanyCommandHandler : IRequestHandler<ApproveCompanyCommand, CompanyDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public ApproveCompanyCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<CompanyDto> Handle(ApproveCompanyCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var company = await _db.Set<Company>()
                              .FirstOrDefaultAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken)
                          ?? throw new KeyNotFoundException("Компания не найдена.");

            var oldValue = new
                { company.Status, company.ModerationStatus, company.ModerationComment, company.VerifiedAt };
            company.ModerationStatus = ModerationStatuses.Approved;
            company.ModerationComment = request.Comment;
            company.ModeratedByUserId = userId;
            company.ModeratedAt = DateTimeOffset.UtcNow;
            company.Status = "Verified";
            company.VerifiedAt ??= DateTimeOffset.UtcNow;
            company.UpdatedAt = DateTimeOffset.UtcNow;

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.CompanyApproved,
                EntityType: nameof(Company),
                EntityId: company.Id,
                OldValue: oldValue,
                NewValue: new
                    { company.Status, company.ModerationStatus, company.ModerationComment, company.VerifiedAt },
                UserId: userId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new CompanyDto(company.Id, company.Name, company.Type,company.Description, company.Industry, company.Website,
                company.LogoFileId, company.Status, company.VerifiedAt, company.ModerationStatus,
                company.ModerationComment, company.ModeratedByUserId, company.ModeratedAt);
        }
    }
}