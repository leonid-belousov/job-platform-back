using System.Text.Json;
using JobPlatform.BLL.Common.Interfaces;
using JobPlatform.BLL.Common.Models;
using JobPlatform.Core.Entities.Audit;
using JobPlatform.DAL.Interfaces;

namespace JobPlatform.BLL.Common.Audit;

public sealed class AuditService : IAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AuditService(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        var userId = auditEvent.UserId ?? _currentUser.UserId;

        await _db.Set<AuditLog>().AddAsync(new AuditLog
        {
            UserId = userId,
            Action = auditEvent.Action,
            EntityType = auditEvent.EntityType,
            EntityId = auditEvent.EntityId,
            OldValue = SerializeOrNull(auditEvent.OldValue),
            NewValue = SerializeOrNull(auditEvent.NewValue),
            IpAddress = _currentUser.IpAddress,
            UserAgent = _currentUser.UserAgent
        }, cancellationToken);
    }

    private static string? SerializeOrNull(object? value)
    {
        return value is null ? null : JsonSerializer.Serialize(value, JsonOptions);
    }
}
