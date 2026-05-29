using JobPlatform.BLL.CQRS.Vacancies.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Vacancies.Commands.ArchiveVacancy;

public sealed record ArchiveVacancyCommand(Guid VacancyId) : IRequest<VacancyDto>
{
    public class ArchiveVacancyCommandHandler : IRequestHandler<ArchiveVacancyCommand, VacancyDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public ArchiveVacancyCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<VacancyDto> Handle(ArchiveVacancyCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            
            var vacancy = await _db.Set<JobVacancy>()
                              .FirstOrDefaultAsync(x => x.Id == request.VacancyId && !x.IsDeleted, cancellationToken)
                          ?? throw new InvalidOperationException("Вакансия не найдена.");

            var hasAccess = await _db.Set<CompanyMember>()
                .AnyAsync(x => x.CompanyId == vacancy.CompanyId && x.UserId == userId && x.Status == "Active",
                    cancellationToken);

            if (!hasAccess) throw new UnauthorizedAccessException("Нет доступа к вакансии.");

            vacancy.Status = "Archived";
            vacancy.ArchivedAt = DateTimeOffset.UtcNow;
            vacancy.UpdatedAt = DateTimeOffset.UtcNow;
            
            await _db.SaveChangesAsync(cancellationToken);
            
            return new VacancyDto(vacancy.Id, vacancy.Title, vacancy.City, vacancy.SalaryFrom, vacancy.SalaryTo,
                vacancy.Status);
        }
    }
}