using JobPlatform.BLL.Common.Models;

namespace JobPlatform.BLL.Common.Interfaces;

public interface IAuditService
{
    Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
}