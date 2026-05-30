using JobPlatform.BLL.Common.Email;

namespace JobPlatform.BLL.Common.Interfaces;

public interface IEmailTemplateRenderer
{
    EmailTemplateResult RenderApplicationCreated(string vacancyTitle, string candidateName, string? coverLetter);

    EmailTemplateResult RenderApplicationStatusChanged(string vacancyTitle, string oldStatus, string newStatus, string? comment);
}
