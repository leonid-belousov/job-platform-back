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

        public CreateCompanyCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
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
            
            await _db.SaveChangesAsync(cancellationToken);

            return new CompanyDto(company.Id, company.Name, company.Description, company.Industry, company.Website,
                company.Status, company.VerifiedAt);
        }
    }
}