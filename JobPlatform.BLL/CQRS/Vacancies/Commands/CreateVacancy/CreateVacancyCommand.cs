using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Vacancies.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Vacancies.Commands.CreateVacancy;

public record CreateVacancyCommand(
    Guid CompanyId,
    string Title,
    string Description,
    string? City,
    decimal? SalaryFrom,
    decimal? SalaryTo) : IRequest<VacancyDto>
{
    public class CreateVacancyCommandHandler : IRequestHandler<CreateVacancyCommand, VacancyDto>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;

        public CreateVacancyCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService,
            IAuditService auditService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _auditService = auditService;
        }

        public async Task<VacancyDto> Handle(CreateVacancyCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
            var isCompanyMember = await _dbContext.Set<CompanyMember>().AnyAsync(
                x => x.CompanyId == request.CompanyId && x.UserId == userId && x.Status == "Active", cancellationToken);

            if (!isCompanyMember) throw new UnauthorizedAccessException("Нет доступа к компании.");

            var vacancy = new JobVacancy
            {
                CompanyId = request.CompanyId, CreatedByUserId = userId, Title = request.Title,
                Description = request.Description, City = request.City, SalaryFrom = request.SalaryFrom,
                SalaryTo = request.SalaryTo, Status = "Draft"
            };

            await _dbContext.Set<JobVacancy>().AddAsync(vacancy, cancellationToken);

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.VacancyCreated,
                EntityType: nameof(JobVacancy),
                EntityId: vacancy.Id,
                NewValue: new
                {
                    vacancy.CompanyId, vacancy.Title, vacancy.City, vacancy.SalaryFrom, vacancy.SalaryTo, vacancy.Status
                },
                UserId: userId), cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new VacancyDto(vacancy.Id, vacancy.Title, vacancy.City, vacancy.EmploymentType, vacancy.WorkFormat, vacancy.ExperienceLevel, vacancy.SalaryFrom, vacancy.SalaryTo, vacancy.Currency, vacancy.Status);
        }
    }
}