namespace JobPlatform.BLL.Common.Email;

public sealed record EmailTemplateResult(
    string Subject,
    string HtmlBody,
    string TextBody);
