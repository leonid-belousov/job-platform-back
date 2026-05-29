using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Users;

public sealed class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}