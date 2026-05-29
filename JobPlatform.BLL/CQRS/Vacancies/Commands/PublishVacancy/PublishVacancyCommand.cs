using JobPlatform.BLL.CQRS.Vacancies.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Vacancies.Commands.PublishVacancy;

public sealed record PublishVacancyCommand(Guid VacancyId) : IRequest<VacancyDto>
{
    public class PublishVacancyCommandHandler : IRequestHandler<PublishVacancyCommand, VacancyDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public PublishVacancyCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        public async Task<VacancyDto> Handle(PublishVacancyCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

            var vacancy = await _db.Set<JobVacancy>()
                              .FirstOrDefaultAsync(x => x.Id == request.VacancyId && !x.IsDeleted, cancellationToken)
                          ?? throw new InvalidOperationException("Вакансия не найдена.");

            var hasAccess = await _db.Set<CompanyMember>()
                .AnyAsync(x => x.CompanyId == vacancy.CompanyId && x.UserId == userId && x.Status == "Active",
                    cancellationToken);

            if (!hasAccess) throw new UnauthorizedAccessException("Нет доступа к вакансии.");

            if (string.IsNullOrWhiteSpace(vacancy.Title) || string.IsNullOrWhiteSpace(vacancy.Description))
                throw new InvalidOperationException("Для публикации вакансии обязательны название и описание.");

            vacancy.Status = "Published";
            vacancy.PublishedAt = DateTimeOffset.UtcNow;
            vacancy.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return new VacancyDto(vacancy.Id, vacancy.Title, vacancy.City, vacancy.SalaryFrom, vacancy.SalaryTo,
                vacancy.Status);
        }
    }
}