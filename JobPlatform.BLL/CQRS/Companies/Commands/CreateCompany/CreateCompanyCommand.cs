using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Companies.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.DAL.Interfaces;
using MediatR;

namespace JobPlatform.BLL.CQRS.Companies.Commands.CreateCompany;

public sealed record CreateCompanyCommand(
    string Name,
    string? Description,
    string? Industry,
    string? Website) : IRequest<CompanyDto>
{
    public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, CompanyDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public CreateCompanyCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser,
            IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new InvalidOperationException("Название компании обязательно.");

            var company = new Company
            {
                Name = request.Name.Trim(),
                Description = request.Description,
                Industry = request.Industry,
                Website = request.Website,
                Status = "PendingVerification"
            };

            company.Members.Add(new CompanyMember
            {
                Company = company,
                UserId = userId,
                RoleInCompany = "Owner",
                Status = "Active"
            });

            await _db.Set<Company>().AddAsync(company, cancellationToken);

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.CompanyCreated,
                EntityType: nameof(Company),
                EntityId: company.Id,
                NewValue: new { company.Name, company.Industry, company.Status },
                UserId: userId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            return new CompanyDto(company.Id, company.Name, company.Description, company.Industry, company.Website, company.LogoFileId, company.Status, company.VerifiedAt, company.ModerationStatus, company.ModerationComment, company.ModeratedByUserId, company.ModeratedAt);
        }
    }
}