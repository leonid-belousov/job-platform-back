using JobPlatform.Core.Entities.Common;

namespace JobPlatform.Core.Entities.Users;

public sealed class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}