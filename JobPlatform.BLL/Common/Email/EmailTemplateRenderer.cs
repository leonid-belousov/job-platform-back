using System.Net;
using JobPlatform.BLL.Common.Interfaces;

namespace JobPlatform.BLL.Common.Email;

public sealed class EmailTemplateRenderer : IEmailTemplateRenderer
{
    public EmailTemplateResult RenderApplicationCreated(string vacancyTitle, string candidateName, string? coverLetter)
    {
        var safeVacancyTitle = Html(vacancyTitle);
        var safeCandidateName = Html(candidateName);
        var safeCoverLetter = string.IsNullOrWhiteSpace(coverLetter) ? null : Html(coverLetter);

        var subject = $"Новый отклик на вакансию: {vacancyTitle}";
        var textBody = $"Кандидат {candidateName} откликнулся на вакансию '{vacancyTitle}'." +
                       (string.IsNullOrWhiteSpace(coverLetter)
                           ? string.Empty
                           : $"\n\nСопроводительное письмо:\n{coverLetter}");

        var htmlBody = Wrap(
            "Новый отклик на вакансию",
            $"<p>Кандидат <strong>{safeCandidateName}</strong> откликнулся на вакансию <strong>{safeVacancyTitle}</strong>.</p>" +
            (safeCoverLetter is null
                ? string.Empty
                : $"<p><strong>Сопроводительное письмо:</strong></p><blockquote>{safeCoverLetter.Replace("\n", "<br />")}</blockquote>"));

        return new EmailTemplateResult(subject, htmlBody, textBody);
    }

    public EmailTemplateResult RenderApplicationStatusChanged(string vacancyTitle, string oldStatus, string newStatus,
        string? comment)
    {
        var safeVacancyTitle = Html(vacancyTitle);
        var safeOldStatus = Html(oldStatus);
        var safeNewStatus = Html(newStatus);
        var safeComment = string.IsNullOrWhiteSpace(comment) ? null : Html(comment);

        var subject = $"Статус отклика изменен: {vacancyTitle}";
        var textBody = $"Статус отклика на вакансию '{vacancyTitle}' изменен: {oldStatus} -> {newStatus}." +
                       (string.IsNullOrWhiteSpace(comment) ? string.Empty : $"\n\nКомментарий:\n{comment}");

        var htmlBody = Wrap(
            "Статус отклика изменен",
            $"<p>Статус отклика на вакансию <strong>{safeVacancyTitle}</strong> изменен:</p>" +
            $"<p><strong>{safeOldStatus}</strong> → <strong>{safeNewStatus}</strong></p>" +
            (safeComment is null
                ? string.Empty
                : $"<p><strong>Комментарий:</strong></p><blockquote>{safeComment.Replace("\n", "<br />")}</blockquote>"));

        return new EmailTemplateResult(subject, htmlBody, textBody);
    }

    public EmailTemplateResult RenderEmailConfirmation(string confirmationUrl)
    {
        var safeUrl = Html(confirmationUrl);
        var subject = "Подтверждение email";
        var textBody = $"Подтвердите email, перейдя по ссылке: {confirmationUrl}";
        var htmlBody = Wrap(
            "Подтвердите email",
            $"<p>Для завершения регистрации подтвердите email.</p><p><a href=\"{safeUrl}\">Подтвердить email</a></p>");

        return new EmailTemplateResult(subject, htmlBody, textBody);
    }

    public EmailTemplateResult RenderPasswordReset(string resetUrl)
    {
        var safeUrl = Html(resetUrl);
        var subject = "Восстановление пароля";
        var textBody = $"Для смены пароля перейдите по ссылке: {resetUrl}";
        var htmlBody = Wrap(
            "Восстановление пароля",
            $"<p>Для смены пароля перейдите по ссылке ниже. Если вы не запрашивали восстановление, проигнорируйте письмо.</p><p><a href=\"{safeUrl}\">Сменить пароль</a></p>");

        return new EmailTemplateResult(subject, htmlBody, textBody);
    }

    public EmailTemplateResult RenderInterviewInvitationCreated(string vacancyTitle, DateTimeOffset scheduledAt,
        string format, string? location, string? meetingUrl, string? message)
    {
        var subject = $"Приглашение на интервью: {vacancyTitle}";
        var textBody = $"Вас пригласили на интервью по вакансии '{vacancyTitle}'.\nДата: {scheduledAt:u}\nФормат: {format}" +
                       (string.IsNullOrWhiteSpace(location) ? string.Empty : $"\nМесто: {location}") +
                       (string.IsNullOrWhiteSpace(meetingUrl) ? string.Empty : $"\nСсылка: {meetingUrl}") +
                       (string.IsNullOrWhiteSpace(message) ? string.Empty : $"\n\nСообщение:\n{message}");
        var htmlBody = Wrap("Приглашение на интервью",
            $"<p>Вас пригласили на интервью по вакансии <strong>{Html(vacancyTitle)}</strong>.</p>" +
            $"<p><strong>Дата:</strong> {Html(scheduledAt.ToString("u"))}<br /><strong>Формат:</strong> {Html(format)}</p>" +
            (string.IsNullOrWhiteSpace(location) ? string.Empty : $"<p><strong>Место:</strong> {Html(location)}</p>") +
            (string.IsNullOrWhiteSpace(meetingUrl) ? string.Empty : $"<p><strong>Ссылка:</strong> <a href=\"{Html(meetingUrl)}\">{Html(meetingUrl)}</a></p>") +
            (string.IsNullOrWhiteSpace(message) ? string.Empty : $"<blockquote>{Html(message).Replace("\n", "<br />")}</blockquote>"));
        return new EmailTemplateResult(subject, htmlBody, textBody);
    }

    public EmailTemplateResult RenderInterviewInvitationResponded(string vacancyTitle, string candidateName,
        string responseStatus, DateTimeOffset scheduledAt)
    {
        var subject = $"Ответ на приглашение: {vacancyTitle}";
        var textBody = $"Кандидат {candidateName} ответил на приглашение по вакансии '{vacancyTitle}': {responseStatus}. Дата интервью: {scheduledAt:u}.";
        var htmlBody = Wrap("Ответ на приглашение",
            $"<p>Кандидат <strong>{Html(candidateName)}</strong> ответил на приглашение по вакансии <strong>{Html(vacancyTitle)}</strong>: <strong>{Html(responseStatus)}</strong>.</p>" +
            $"<p><strong>Дата интервью:</strong> {Html(scheduledAt.ToString("u"))}</p>");
        return new EmailTemplateResult(subject, htmlBody, textBody);
    }

    public EmailTemplateResult RenderNewApplicationsReminder(string vacancyTitle, int newApplicationsCount)
    {
        var subject = $"Новые отклики ожидают обработки: {vacancyTitle}";
        var textBody = $"По вакансии '{vacancyTitle}' есть новые отклики без обработки: {newApplicationsCount}.";
        var htmlBody = Wrap("Новые отклики ожидают обработки",
            $"<p>По вакансии <strong>{Html(vacancyTitle)}</strong> есть новые отклики без обработки: <strong>{newApplicationsCount}</strong>.</p>");
        return new EmailTemplateResult(subject, htmlBody, textBody);
    }

    public EmailTemplateResult RenderInactiveVacancyReminder(string vacancyTitle, DateTimeOffset? lastActivityAt)
    {
        var subject = $"Вакансия давно без активности: {vacancyTitle}";
        var lastActivity = lastActivityAt?.ToString("u") ?? "нет активности";
        var textBody = $"Вакансия '{vacancyTitle}' давно без активности. Последняя активность: {lastActivity}.";
        var htmlBody = Wrap("Вакансия давно без активности",
            $"<p>Вакансия <strong>{Html(vacancyTitle)}</strong> давно без активности.</p><p><strong>Последняя активность:</strong> {Html(lastActivity)}</p>");
        return new EmailTemplateResult(subject, htmlBody, textBody);
    }

    private static string Html(string value) => WebUtility.HtmlEncode(value);

    private static string Wrap(string title, string body) =>
        $$"""
          <!doctype html>
          <html lang="ru">
          <head>
            <meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>{{Html(title)}}</title>
          </head>
          <body style="font-family: Arial, sans-serif; color: #111827; line-height: 1.5; margin: 0; padding: 24px; background: #f9fafb;">
            <div style="max-width: 640px; margin: 0 auto; background: #ffffff; border-radius: 12px; padding: 24px; border: 1px solid #e5e7eb;">
              <h1 style="font-size: 20px; margin: 0 0 16px 0;">{{Html(title)}}</h1>
              {{body}}
              <hr style="border: 0; border-top: 1px solid #e5e7eb; margin: 24px 0;" />
              <p style="font-size: 12px; color: #6b7280; margin: 0;">Это автоматическое уведомление платформы подбора персонала.</p>
            </div>
          </body>
          </html>
          """;
}
