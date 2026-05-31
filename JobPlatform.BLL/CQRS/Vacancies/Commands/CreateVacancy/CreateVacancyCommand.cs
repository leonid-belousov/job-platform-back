using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Vacancies.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Dictionaries;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Vacancies.Commands.CreateVacancy;

public record CreateVacancyCommand(
    Guid CompanyId,
    string Title,
    string Description,
    string Requirements,
    string? Responsibilities,
    string Conditions,
    string Country,
    string? City,
    string? EmploymentType,
    string? WorkFormat,
    string? ExperienceLevel,
    decimal? SalaryFrom,
    decimal? SalaryTo,
    string? Currency) : IRequest<VacancyDto>
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

            await ValidateDictionaryValueAsync(DictionaryTypes.Country, request.Country, cancellationToken);
            await ValidateDictionaryValueAsync(DictionaryTypes.City, NormalizeOptional(request.City), cancellationToken);
            await ValidateDictionaryValueAsync(DictionaryTypes.EmploymentType, NormalizeOptional(request.EmploymentType),
                cancellationToken);
            await ValidateDictionaryValueAsync(DictionaryTypes.WorkFormat, NormalizeOptional(request.WorkFormat), cancellationToken);
            await ValidateDictionaryValueAsync(DictionaryTypes.ExperienceLevel, NormalizeOptional(request.ExperienceLevel),
                cancellationToken);
            await ValidateDictionaryValueAsync(DictionaryTypes.Currency, NormalizeOptional(request.Currency), cancellationToken);

            var vacancy = new JobVacancy
            {
                CompanyId = request.CompanyId,
                CreatedByUserId = userId,
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Requirements = request.Requirements.Trim(),
                Responsibilities = string.IsNullOrWhiteSpace(request.Responsibilities) ? null : request.Responsibilities.Trim(),
                Conditions = request.Conditions.Trim(),
                Country = request.Country.Trim().ToLowerInvariant(),
                City = NormalizeOptional(request.City),
                EmploymentType = NormalizeOptional(request.EmploymentType),
                WorkFormat = NormalizeOptional(request.WorkFormat),
                ExperienceLevel = NormalizeOptional(request.ExperienceLevel),
                SalaryFrom = request.SalaryFrom,
                SalaryTo = request.SalaryTo,
                Currency = NormalizeOptional(request.Currency),
                Status = "Draft"
            };

            await _dbContext.Set<JobVacancy>().AddAsync(vacancy, cancellationToken);

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.VacancyCreated,
                EntityType: nameof(JobVacancy),
                EntityId: vacancy.Id,
                NewValue: new
                {
                    vacancy.CompanyId, vacancy.Title, vacancy.Description, vacancy.Requirements, vacancy.Responsibilities,
                    vacancy.Conditions, vacancy.Country, vacancy.City, vacancy.EmploymentType, vacancy.WorkFormat,
                    vacancy.ExperienceLevel, vacancy.SalaryFrom, vacancy.SalaryTo, vacancy.Currency, vacancy.Status
                },
                UserId: userId), cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new VacancyDto(vacancy.Id, vacancy.Title, vacancy.City, vacancy.EmploymentType, vacancy.WorkFormat,
                vacancy.ExperienceLevel, vacancy.SalaryFrom, vacancy.SalaryTo, vacancy.Currency, vacancy.Status,
                vacancy.ModerationStatus, vacancy.ModerationComment, vacancy.ModeratedByUserId, vacancy.ModeratedAt,
                Description: vacancy.Description,
                Requirements: vacancy.Requirements,
                Responsibilities: vacancy.Responsibilities,
                Conditions: vacancy.Conditions,
                Country: vacancy.Country);
        }

        private static string? NormalizeOptional(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

        private async Task ValidateDictionaryValueAsync(string type, string? code, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(code)) return;

            var exists = await _dbContext.Set<DictionaryItem>()
                .AnyAsync(x => x.Type == type && x.Code == code && x.IsActive, cancellationToken);
            if (!exists)
                throw new InvalidOperationException(
                    $"Dictionary value '{code}' is not active or does not exist for type '{type}'.");
        }
    }
}