using JobPlatform.BLL.CQRS.Vacancies.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Vacancies.Commands.ExtendVacancy;

public sealed record ExtendVacancyCommand(Guid VacancyId, int Days) : IRequest<VacancyDto>
{
    public sealed class Handler : IRequestHandler<ExtendVacancyCommand, VacancyDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<VacancyDto> Handle(ExtendVacancyCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var vacancy = await _db.Set<JobVacancy>()
                              .FirstOrDefaultAsync(x => x.Id == request.VacancyId && !x.IsDeleted, cancellationToken)
                          ?? throw new InvalidOperationException("Vacancy not found.");

            var hasAccess = await _db.Set<CompanyMember>()
                .AnyAsync(x => x.CompanyId == vacancy.CompanyId && x.UserId == userId && x.Status == "Active",
                    cancellationToken);

            if (!hasAccess)
            {
                throw new UnauthorizedAccessException();
            }

            if (vacancy.Status != "Published")
            {
                throw new InvalidOperationException("Only published vacancies can be extended.");
            }

            if (vacancy.ModerationStatus == "Rejected")
            {
                throw new InvalidOperationException("Rejected vacancy cannot be extended.");
            }

            if (vacancy.ExtensionCount >= 3)
            {
                throw new InvalidOperationException("Vacancy extension limit reached.");
            }

            var now = DateTimeOffset.UtcNow;
            var baseDate = vacancy.ExpiresAt.HasValue && vacancy.ExpiresAt.Value > now ? vacancy.ExpiresAt.Value : now;
            vacancy.ExpiresAt = baseDate.AddDays(request.Days);
            vacancy.ExtendedAt = now;
            vacancy.ExtensionCount += 1;
            vacancy.UpdatedAt = now;

            await _db.SaveChangesAsync(cancellationToken);

            return new VacancyDto(vacancy.Id, vacancy.Title, vacancy.City, vacancy.EmploymentType, vacancy.WorkFormat,
                vacancy.ExperienceLevel, vacancy.SalaryFrom, vacancy.SalaryTo, vacancy.Currency, vacancy.Status,
                vacancy.ModerationStatus, vacancy.ModerationComment, vacancy.ModeratedByUserId, vacancy.ModeratedAt,
                vacancy.PublishedAt, vacancy.ExpiresAt, vacancy.ExtendedAt, vacancy.ExtensionCount,
                vacancy.Description, vacancy.Requirements, vacancy.Responsibilities, vacancy.Conditions, vacancy.Country);
        }
    }
}