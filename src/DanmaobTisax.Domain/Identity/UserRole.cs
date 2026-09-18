using DanmaobTisax.Domain.Common;
using DanmaobTisax.Domain.Auditing;

namespace DanmaobTisax.Domain.Identity;

public class UserRole : BaseEntity, IAuditable
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAtUtc { get; set; }

    protected UserRole()
    {
    }

    public UserRole(Guid userId, Guid roleId)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedAtUtc = DateTime.UtcNow;
    }
}
