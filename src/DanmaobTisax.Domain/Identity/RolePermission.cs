namespace DanmaobTisax.Domain.Identity;

using DanmaobTisax.Domain.Common;
using DanmaobTisax.Domain.Auditing;

public class RolePermission : BaseEntity, IAuditable
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public DateTime GrantedAtUtc { get; set; }

    protected RolePermission()
    {
    }

    public RolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
        GrantedAtUtc = DateTime.UtcNow;
    }
}
