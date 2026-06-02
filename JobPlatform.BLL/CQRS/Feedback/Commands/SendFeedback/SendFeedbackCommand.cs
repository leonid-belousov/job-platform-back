using System.Net;
using System.Text;
using JobPlatform.DAL.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace JobPlatform.BLL.CQRS.Feedback.Commands.SendFeedback;

public sealed record SendFeedbackCommand(
    string Name,
    string Phone,
    string Email,
    string? Company,
    string Message) : IRequest;

public sealed class SendFeedbackCommandHandler : IRequestHandler<SendFeedbackCommand>
{
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;

    public SendFeedbackCommandHandler(IConfiguration configuration, IEmailSender emailSender)
    {
        _configuration = configuration;
        _emailSender = emailSender;
    }

    public Task Handle(SendFeedbackCommand request, CancellationToken cancellationToken)
    {
        var recipient = _configuration["Feedback:RecipientEmail"]
                        ?? _configuration["Email:FromEmail"]
                        ?? throw new InvalidOperationException("Feedback:RecipientEmail or Email:FromEmail must be configured.");

        var normalizedName = request.Name.Trim();
        var subject = $"Новое обращение с главной страницы: {normalizedName}";
        var textBody = BuildTextBody(request);
        var htmlBody = BuildHtmlBody(request);

        return _emailSender.SendAsync(recipient, subject, htmlBody, textBody, cancellationToken);
    }

    private static string BuildTextBody(SendFeedbackCommand request)
        => new StringBuilder()
            .AppendLine("Новое обращение с формы обратной связи")
            .AppendLine()
            .AppendLine($"Имя: {request.Name.Trim()}")
            .AppendLine($"Телефон: {request.Phone.Trim()}")
            .AppendLine($"E-mail: {request.Email.Trim()}")
            .AppendLine($"Компания: {NormalizeOptionalValue(request.Company)}")
            .AppendLine()
            .AppendLine("Сообщение:")
            .AppendLine(request.Message.Trim())
            .ToString();

    private static string BuildHtmlBody(SendFeedbackCommand request)
    {
        var rows = new[]
        {
            BuildRow("Имя", request.Name),
            BuildRow("Телефон", request.Phone),
            BuildRow("E-mail", request.Email),
            BuildRow("Компания", NormalizeOptionalValue(request.Company)),
        };

        return $"""
               <h2>Новое обращение с формы обратной связи</h2>
               <table cellpadding="8" cellspacing="0" style="border-collapse:collapse;border:1px solid #e5e7eb;">
                   {string.Join(string.Empty, rows)}
               </table>
               <h3>Сообщение</h3>
               <p>{EncodeMultiline(request.Message)}</p>
               """;
    }

    private static string BuildRow(string label, string value)
        => $"""
           <tr>
               <td style="border:1px solid #e5e7eb;font-weight:700;">{WebUtility.HtmlEncode(label)}</td>
               <td style="border:1px solid #e5e7eb;">{WebUtility.HtmlEncode(value.Trim())}</td>
           </tr>
           """;

    private static string NormalizeOptionalValue(string? value)
        => string.IsNullOrWhiteSpace(value) ? "Не указана" : value.Trim();

    private static string EncodeMultiline(string value)
        => WebUtility.HtmlEncode(value.Trim()).Replace("\n", "<br />");
}
