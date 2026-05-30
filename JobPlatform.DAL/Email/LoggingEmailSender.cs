using JobPlatform.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobPlatform.DAL.Email;

public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;
    private readonly EmailOptions _options;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger, IOptions<EmailOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        // MVP-заглушка. Реальная SMTP/SendGrid/Mailgun-интеграция добавляется после выбора провайдера.
        // TODO: Необходимо добавить боевой аггрегатор 
        _logger.LogInformation(
            "Email notification stub. Enabled={Enabled}, From={From}, To={To}, Subject={Subject}, Body={Body}",
            _options.Enabled,
            _options.From,
            to,
            subject,
            body);

        return Task.CompletedTask;
    }
}