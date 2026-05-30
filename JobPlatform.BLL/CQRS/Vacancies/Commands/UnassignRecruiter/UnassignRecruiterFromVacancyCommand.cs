using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Vacancies.Commands.UnassignRecruiter;

public sealed record UnassignRecruiterFromVacancyCommand(Guid VacancyId, Guid RecruiterUserId) : IRequest
{
    public sealed class Handler : IRequestHandler<UnassignRecruiterFromVacancyCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task Handle(UnassignRecruiterFromVacancyCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

            var vacancy = await _db.Set<JobVacancy>().AsNoTracking()
                              .FirstOrDefaultAsync(x => x.Id == request.VacancyId && !x.IsDeleted, cancellationToken)
                          ?? throw new InvalidOperationException("Vacancy not found.");

            var hasAccess = await _db.Set<CompanyMember>()
                .AnyAsync(x => x.CompanyId == vacancy.CompanyId && x.UserId == currentUserId && x.Status == "Active",
                    cancellationToken);

            if (!hasAccess)
            {
                throw new UnauthorizedAccessException();
            }

            var assignment = await _db.Set<VacancyRecruiter>()
                .FirstOrDefaultAsync(x => x.VacancyId == request.VacancyId && x.RecruiterUserId == request.RecruiterUserId,
                    cancellationToken)
                ?? throw new InvalidOperationException("Recruiter assignment not found.");

            assignment.Status = "inactive";
            assignment.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}