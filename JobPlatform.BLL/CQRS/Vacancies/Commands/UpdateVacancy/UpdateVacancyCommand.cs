using JobPlatform.BLL.Common.Audit;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.BLL.CQRS.Vacancies.DTO;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Dictionaries;
using JobPlatform.Core.Entities.Moderation;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.BLL.CQRS.Vacancies.Commands.UpdateVacancy;

public sealed record UpdateVacancyCommand(
    Guid VacancyId,
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
    public sealed class Handler : IRequestHandler<UpdateVacancyCommand, VacancyDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;

        public Handler(IApplicationDbContext db, ICurrentUserService currentUser, IAuditService auditService)
        {
            _db = db;
            _currentUser = currentUser;
            _auditService = auditService;
        }

        public async Task<VacancyDto> Handle(UpdateVacancyCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
            var vacancy = await _db.Set<JobVacancy>()
                              .FirstOrDefaultAsync(x => x.Id == request.VacancyId && !x.IsDeleted, cancellationToken)
                          ?? throw new InvalidOperationException("Вакансия не найдена.");

            var hasAccess = await _db.Set<CompanyMember>().AnyAsync(
                x => x.CompanyId == vacancy.CompanyId && x.UserId == userId && x.Status == "Active",
                cancellationToken);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("Нет доступа к вакансии.");
            }

            await ValidateDictionaryValueAsync(DictionaryTypes.Country, request.Country, cancellationToken);
            await ValidateDictionaryValueAsync(DictionaryTypes.City, NormalizeOptional(request.City), cancellationToken);
            await ValidateDictionaryValueAsync(DictionaryTypes.EmploymentType, NormalizeOptional(request.EmploymentType), cancellationToken);
            await ValidateDictionaryValueAsync(DictionaryTypes.WorkFormat, NormalizeOptional(request.WorkFormat), cancellationToken);
            await ValidateDictionaryValueAsync(DictionaryTypes.ExperienceLevel, NormalizeOptional(request.ExperienceLevel), cancellationToken);
            await ValidateDictionaryValueAsync(DictionaryTypes.Currency, NormalizeOptional(request.Currency), cancellationToken);

            var oldValue = new
            {
                vacancy.Title,
                vacancy.Description,
                vacancy.Requirements,
                vacancy.Responsibilities,
                vacancy.Conditions,
                vacancy.Country,
                vacancy.City,
                vacancy.EmploymentType,
                vacancy.WorkFormat,
                vacancy.ExperienceLevel,
                vacancy.SalaryFrom,
                vacancy.SalaryTo,
                vacancy.Currency,
                vacancy.Status,
                vacancy.ModerationStatus
            };

            vacancy.Title = request.Title.Trim();
            vacancy.Description = request.Description.Trim();
            vacancy.Requirements = request.Requirements.Trim();
            vacancy.Responsibilities = string.IsNullOrWhiteSpace(request.Responsibilities) ? null : request.Responsibilities.Trim();
            vacancy.Conditions = request.Conditions.Trim();
            vacancy.Country = request.Country.Trim().ToLowerInvariant();
            vacancy.City = NormalizeOptional(request.City);
            vacancy.EmploymentType = NormalizeOptional(request.EmploymentType);
            vacancy.WorkFormat = NormalizeOptional(request.WorkFormat);
            vacancy.ExperienceLevel = NormalizeOptional(request.ExperienceLevel);
            vacancy.SalaryFrom = request.SalaryFrom;
            vacancy.SalaryTo = request.SalaryTo;
            vacancy.Currency = NormalizeOptional(request.Currency);

            if (vacancy.ModerationStatus == ModerationStatuses.Approved || vacancy.Status == "Published")
            {
                vacancy.Status = "PendingModeration";
                vacancy.ModerationStatus = ModerationStatuses.Pending;
                vacancy.ModerationComment = null;
                vacancy.ModeratedAt = null;
                vacancy.ModeratedByUserId = null;
            }

            vacancy.UpdatedAt = DateTimeOffset.UtcNow;

            await _auditService.AddAsync(new AuditEvent(
                AuditActions.VacancyUpdated,
                EntityType: nameof(JobVacancy),
                EntityId: vacancy.Id,
                OldValue: oldValue,
                NewValue: new
                {
                    vacancy.Title,
                    vacancy.Description,
                    vacancy.Requirements,
                    vacancy.Responsibilities,
                    vacancy.Conditions,
                    vacancy.Country,
                    vacancy.City,
                    vacancy.EmploymentType,
                    vacancy.WorkFormat,
                    vacancy.ExperienceLevel,
                    vacancy.SalaryFrom,
                    vacancy.SalaryTo,
                    vacancy.Currency,
                    vacancy.Status,
                    vacancy.ModerationStatus
                },
                UserId: userId), cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            return new VacancyDto(vacancy.Id, vacancy.Title, vacancy.City, vacancy.EmploymentType, vacancy.WorkFormat,
                vacancy.ExperienceLevel, vacancy.SalaryFrom, vacancy.SalaryTo, vacancy.Currency, vacancy.Status,
                vacancy.ModerationStatus, vacancy.ModerationComment, vacancy.ModeratedByUserId, vacancy.ModeratedAt,
                vacancy.PublishedAt, vacancy.ExpiresAt, vacancy.ExtendedAt, vacancy.ExtensionCount,
                vacancy.Description, vacancy.Requirements, vacancy.Responsibilities, vacancy.Conditions, vacancy.Country);
        }

        private static string? NormalizeOptional(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

        private async Task ValidateDictionaryValueAsync(string type, string? code, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(code)) return;

            var exists = await _db.Set<DictionaryItem>()
                .AnyAsync(x => x.Type == type && x.Code == code && x.IsActive, cancellationToken);
            if (!exists)
            {
                throw new InvalidOperationException(
                    $"Dictionary value '{code}' is not active or does not exist for type '{type}'.");
            }
        }
    }
}