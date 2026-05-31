using JobPlatform.BLL.Common.Email;

namespace JobPlatform.BLL.Common.Interfaces;

public interface IEmailTemplateRenderer
{
    EmailTemplateResult RenderApplicationCreated(string vacancyTitle, string candidateName, string? coverLetter);

    EmailTemplateResult RenderApplicationStatusChanged(string vacancyTitle, string oldStatus, string newStatus, string? comment);

    EmailTemplateResult RenderEmailConfirmation(string confirmationUrl);

    EmailTemplateResult RenderPasswordReset(string resetUrl);

    EmailTemplateResult RenderInterviewInvitationCreated(string vacancyTitle, DateTimeOffset scheduledAt, string format,
        string? location, string? meetingUrl, string? message);

    EmailTemplateResult RenderInterviewInvitationResponded(string vacancyTitle, string candidateName, string responseStatus,
        DateTimeOffset scheduledAt);

    EmailTemplateResult RenderNewApplicationsReminder(string vacancyTitle, int newApplicationsCount);

    EmailTemplateResult RenderInactiveVacancyReminder(string vacancyTitle, DateTimeOffset? lastActivityAt);
}
