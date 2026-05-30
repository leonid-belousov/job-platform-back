using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Companies.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Moderation;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Moderation.Commands.RejectCompany;

public sealed record RejectCompanyCommand(Guid CompanyId, string Comment) : IRequest<CompanyDto>
{
    public class RejectCompanyCommandHandler : IRequestHandler<RejectCompanyCommand, CompanyDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public RejectCompanyCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<CompanyDto> Handle(RejectCompanyCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var company = await _db.Set<Company>()
                              .FirstOrDefaultAsync(x => x.Id == request.CompanyId && !x.IsDeleted, cancellationToken)
                          ?? throw new KeyNotFoundException("Компания не найдена.");

            var oldValue = new
                { company.Status, company.ModerationStatus, company.ModerationComment, company.VerifiedAt };
            company.ModerationStatus = ModerationStatuses.Rejected;
            company.ModerationComment = request.Comment;
            company.ModeratedByUserId = userId;
            company.ModeratedAt = DateTimeOffset.UtcNow;
            company.Status = "Rejected";
            company.UpdatedAt = DateTimeOffset.UtcNow;

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.CompanyRejected,
                EntityType: nameof(Company),
                EntityId: company.Id,
                OldValue: oldValue,
                NewValue: new { company.Status, company.ModerationStatus, company.ModerationComment },
                UserId: userId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
            return new CompanyDto(company.Id, company.Name, company.Description, company.Industry, company.Website,
                company.LogoFileId, company.Status, company.VerifiedAt, company.ModerationStatus,
                company.ModerationComment, company.ModeratedByUserId, company.ModeratedAt);
        }
    }
}