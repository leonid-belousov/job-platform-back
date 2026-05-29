using JobPlatform.BLL.CQRS.Companies.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Companies.Queries.GetMyCompanies;

public sealed record GetMyCompaniesQuery : IRequest<IReadOnlyCollection<CompanyDto>>
{
    public class GetMyCompaniesQueryHandler : IRequestHandler<GetMyCompaniesQuery, IReadOnlyCollection<CompanyDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public GetMyCompaniesQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyCollection<CompanyDto>> Handle(GetMyCompaniesQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            
            return await _db.Set<CompanyMember>()
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.Status == "Active" && !x.Company.IsDeleted)
                .Select(x => new CompanyDto(x.Company.Id, x.Company.Name, x.Company.Description, x.Company.Industry,
                    x.Company.Website, x.Company.Status, x.Company.VerifiedAt))
                .OrderBy(x => x.Name)
                .ToArrayAsync(cancellationToken);
        }
    }
}