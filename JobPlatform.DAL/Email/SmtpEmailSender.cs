using JobPlatform.DAL.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

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

        var message = CreateMessage(to, subject, htmlBody, textBody);
        await SendMessageAsync(message, cancellationToken);
    }

    private MimeMessage CreateMessage(string to, string subject, string htmlBody, string? textBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        if (!string.IsNullOrWhiteSpace(textBody))
        {
            bodyBuilder.TextBody = textBody;
        }

        message.Body = bodyBuilder.ToMessageBody();
        return message;
    }

    private async Task SendMessageAsync(MimeMessage message, CancellationToken cancellationToken)
    {
        try
        {
            using var client = new SmtpClient
            {
                Timeout = Math.Max(_options.TimeoutSeconds, 1) * 1000
            };

            await client.ConnectAsync(
                _options.Host,
                _options.Port,
                GetSecureSocketOptions(),
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation(
                "Email sent. Provider={Provider}, To={To}, Subject={Subject}",
                _options.Provider,
                string.Join(";", message.To.Mailboxes.Select(x => x.Address)),
                message.Subject);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "Email sending canceled. To={To}, Subject={Subject}",
                string.Join(";", message.To.Mailboxes.Select(x => x.Address)),
                message.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Email sending failed. Provider={Provider}, Host={Host}, Port={Port}, To={To}, Subject={Subject}",
                _options.Provider,
                _options.Host,
                _options.Port,
                string.Join(";", message.To.Mailboxes.Select(x => x.Address)),
                message.Subject);
        }
    }

    private SecureSocketOptions GetSecureSocketOptions()
        => _options.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

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
