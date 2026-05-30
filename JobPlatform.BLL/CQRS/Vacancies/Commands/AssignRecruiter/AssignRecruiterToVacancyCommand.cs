using JobPlatform.BLL.CQRS.Vacancies.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Users;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Vacancies.Commands.AssignRecruiter;

public sealed record AssignRecruiterToVacancyCommand(Guid VacancyId, Guid RecruiterUserId)
    : IRequest<VacancyRecruiterDto>
{
    public sealed class Handler : IRequestHandler<AssignRecruiterToVacancyCommand, VacancyRecruiterDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<VacancyRecruiterDto> Handle(AssignRecruiterToVacancyCommand request,
            CancellationToken cancellationToken)
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

            var recruiter = await _db.Set<User>()
                                .Include(x => x.UserRoles)
                                .ThenInclude(x => x.Role)
                                .FirstOrDefaultAsync(x => x.Id == request.RecruiterUserId && !x.IsDeleted,
                                    cancellationToken)
                            ?? throw new InvalidOperationException("Recruiter not found.");

            if (recruiter.UserRoles.All(x => x.Role.Code != "recruiter" && x.Role.Code != "admin"))
            {
                throw new InvalidOperationException("Selected user is not recruiter.");
            }

            var assignment = await _db.Set<VacancyRecruiter>()
                .Include(x => x.RecruiterUser)
                .FirstOrDefaultAsync(x => x.VacancyId == request.VacancyId && x.RecruiterUserId == request.RecruiterUserId,
                    cancellationToken);

            if (assignment is null)
            {
                assignment = new VacancyRecruiter
                {
                    VacancyId = request.VacancyId,
                    RecruiterUserId = request.RecruiterUserId,
                    RecruiterUser = recruiter,
                    AssignedByUserId = currentUserId,
                    Status = "active",
                    AssignedAt = DateTimeOffset.UtcNow
                };
                await _db.Set<VacancyRecruiter>().AddAsync(assignment, cancellationToken);
            }
            else
            {
                assignment.Status = "active";
                assignment.AssignedByUserId = currentUserId;
                assignment.AssignedAt = DateTimeOffset.UtcNow;
                assignment.UpdatedAt = DateTimeOffset.UtcNow;
                assignment.RecruiterUser = recruiter;
            }

            await _db.SaveChangesAsync(cancellationToken);

            return new VacancyRecruiterDto(
                assignment.Id,
                assignment.VacancyId,
                assignment.RecruiterUserId,
                $"{recruiter.FirstName} {recruiter.LastName}".Trim(),
                recruiter.Email,
                assignment.AssignedByUserId,
                assignment.Status,
                assignment.AssignedAt);
        }
    }
}