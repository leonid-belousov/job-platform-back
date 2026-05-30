using System.Net;
using System.Net.Mail;
using JobPlatform.DAL.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobPlatform.DAL.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly ILogger<SmtpEmailSender> _logger;
    private readonly EmailOptions _options;

    public SmtpEmailSender(ILogger<SmtpEmailSender> logger, IOptions<EmailOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Email sending disabled. To={To}, Subject={Subject}", to, subject);
            return;
        }

        if (!ValidateOptions())
        {
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(to));

        if (!string.IsNullOrWhiteSpace(textBody))
        {
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody, null, "text/plain"));
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html"));
        }

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Timeout = Math.Max(_options.TimeoutSeconds, 1) * 1000
        };

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);
        }

        using var registration = cancellationToken.Register(client.SendAsyncCancel);

        try
        {
            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent. Provider={Provider}, To={To}, Subject={Subject}", _options.Provider, to, subject);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Email sending canceled. To={To}, Subject={Subject}", to, subject);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email sending failed. Provider={Provider}, Host={Host}, Port={Port}, To={To}, Subject={Subject}", _options.Provider, _options.Host, _options.Port, to, subject);
            return;
        }
    }

    private bool ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            _logger.LogError("Email.FromEmail is required when email sending is enabled.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            _logger.LogError("Email.Host is required when email sending is enabled.");
            return false;
        }

        if (_options.Port <= 0)
        {
            _logger.LogError("Email.Port must be greater than zero.");
            return false;
        }

        return true;
    }
}
