using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.Core.Entities.Applications;
using JobPlatform.Core.Entities.Companies;
using JobPlatform.Core.Entities.Notifications;
using JobPlatform.Core.Entities.Users;
using JobPlatform.Core.Entities.Vacancies;
using JobPlatform.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobPlatform.BLL.Common.Notifications;

public sealed class SystemNotificationReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SystemNotificationReminderService> _logger;
    private readonly TimeSpan _interval;
    private readonly TimeSpan _newApplicationsAge;
    private readonly TimeSpan _inactiveVacancyAge;
    private readonly TimeSpan _reminderDeduplicationWindow;

    public SystemNotificationReminderService(IServiceScopeFactory scopeFactory,
        ILogger<SystemNotificationReminderService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _interval = TimeSpan.FromHours(GetPositiveInt(configuration, "Notifications:ReminderIntervalHours", 24));
        _newApplicationsAge = TimeSpan.FromHours(GetPositiveInt(configuration, "Notifications:NewApplicationsReminderAgeHours", 24));
        _inactiveVacancyAge = TimeSpan.FromDays(GetPositiveInt(configuration, "Notifications:InactiveVacancyReminderAgeDays", 14));
        _reminderDeduplicationWindow = TimeSpan.FromHours(GetPositiveInt(configuration, "Notifications:ReminderDeduplicationHours", 24));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendRemindersAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System notification reminder processing failed.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task SendRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var emailTemplateRenderer = scope.ServiceProvider.GetRequiredService<IEmailTemplateRenderer>();

        var now = DateTimeOffset.UtcNow;
        var vacancies = await db.Set<JobVacancy>()
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Status == "Published")
            .ToArrayAsync(cancellationToken);

        foreach (var vacancy in vacancies)
        {
            var recipients = await GetVacancyRecipientsAsync(db, vacancy, cancellationToken);
            if (recipients.Length == 0)
            {
                continue;
            }

            await SendNewApplicationsReminderAsync(db, notificationService, emailSender, emailTemplateRenderer,
                vacancy, recipients, now, cancellationToken);

            await SendInactiveVacancyReminderAsync(db, notificationService, emailSender, emailTemplateRenderer,
                vacancy, recipients, now, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SendNewApplicationsReminderAsync(IApplicationDbContext db,
        INotificationService notificationService,
        IEmailSender emailSender,
        IEmailTemplateRenderer emailTemplateRenderer,
        JobVacancy vacancy,
        IReadOnlyCollection<User> recipients,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var threshold = now.Subtract(_newApplicationsAge);
        var newApplicationsCount = await db.Set<JobApplication>()
            .CountAsync(x => !x.IsDeleted
                             && x.VacancyId == vacancy.Id
                             && x.Status == ApplicationStatuses.New
                             && x.CreatedAt <= threshold,
                cancellationToken);

        if (newApplicationsCount <= 0 || await HasRecentReminderAsync(db, NotificationTypes.ReminderNewApplications,
                vacancy.Id, now, cancellationToken))
        {
            return;
        }

        var title = "Новые отклики ожидают обработки";
        var message = $"По вакансии '{vacancy.Title}' есть новые отклики без обработки: {newApplicationsCount}.";
        await notificationService.CreateInternalForUsersAsync(
            recipients.Select(x => x.Id),
            NotificationTypes.ReminderNewApplications,
            title,
            message,
            nameof(JobVacancy),
            vacancy.Id,
            cancellationToken);

        var emailTemplate = emailTemplateRenderer.RenderNewApplicationsReminder(vacancy.Title, newApplicationsCount);
        foreach (var recipient in recipients.Where(x => !string.IsNullOrWhiteSpace(x.Email)))
        {
            await emailSender.SendAsync(recipient.Email, emailTemplate.Subject, emailTemplate.HtmlBody,
                emailTemplate.TextBody, cancellationToken);
        }
    }

    private async Task SendInactiveVacancyReminderAsync(IApplicationDbContext db,
        INotificationService notificationService,
        IEmailSender emailSender,
        IEmailTemplateRenderer emailTemplateRenderer,
        JobVacancy vacancy,
        IReadOnlyCollection<User> recipients,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var lastApplicationAt = await db.Set<JobApplication>()
            .Where(x => !x.IsDeleted && x.VacancyId == vacancy.Id)
            .Select(x => (DateTimeOffset?)x.CreatedAt)
            .MaxAsync(cancellationToken);
        var lastActivityAt = new[] { vacancy.UpdatedAt, vacancy.PublishedAt, lastApplicationAt }
            .Where(x => x.HasValue)
            .Max();

        if (lastActivityAt.HasValue && lastActivityAt.Value > now.Subtract(_inactiveVacancyAge))
        {
            return;
        }

        if (await HasRecentReminderAsync(db, NotificationTypes.ReminderInactiveVacancy, vacancy.Id, now, cancellationToken))
        {
            return;
        }

        var title = "Вакансия давно без активности";
        var message = $"Вакансия '{vacancy.Title}' давно без активности.";
        await notificationService.CreateInternalForUsersAsync(
            recipients.Select(x => x.Id),
            NotificationTypes.ReminderInactiveVacancy,
            title,
            message,
            nameof(JobVacancy),
            vacancy.Id,
            cancellationToken);

        var emailTemplate = emailTemplateRenderer.RenderInactiveVacancyReminder(vacancy.Title, lastActivityAt);
        foreach (var recipient in recipients.Where(x => !string.IsNullOrWhiteSpace(x.Email)))
        {
            await emailSender.SendAsync(recipient.Email, emailTemplate.Subject, emailTemplate.HtmlBody,
                emailTemplate.TextBody, cancellationToken);
        }
    }

    private async Task<User[]> GetVacancyRecipientsAsync(IApplicationDbContext db, JobVacancy vacancy,
        CancellationToken cancellationToken)
    {
        var companyMembers = await db.Set<CompanyMember>()
            .Include(x => x.User)
            .Where(x => x.CompanyId == vacancy.CompanyId && x.Status == "Active" && !x.IsDeleted)
            .Select(x => x.User)
            .ToListAsync(cancellationToken);

        var assignedRecruiters = await db.Set<VacancyRecruiter>()
            .Include(x => x.RecruiterUser)
            .Where(x => x.VacancyId == vacancy.Id && x.Status == "active" && !x.IsDeleted)
            .Select(x => x.RecruiterUser)
            .ToListAsync(cancellationToken);

        return companyMembers
            .Concat(assignedRecruiters)
            .GroupBy(x => x.Id)
            .Select(x => x.First())
            .ToArray();
    }

    private async Task<bool> HasRecentReminderAsync(IApplicationDbContext db, string type, Guid entityId,
        DateTimeOffset now, CancellationToken cancellationToken)
    {
        var since = now.Subtract(_reminderDeduplicationWindow);
        return await db.Set<Notification>().AnyAsync(x => !x.IsDeleted
                                                          && x.Type == type
                                                          && x.EntityId == entityId
                                                          && x.CreatedAt >= since,
            cancellationToken);
    }

    private static int GetPositiveInt(IConfiguration configuration, string key, int defaultValue)
    {
        var value = configuration.GetValue<int?>(key);
        return value.GetValueOrDefault(defaultValue) > 0 ? value.Value : defaultValue;
    }
}
