using DanmaobTisax.Domain.Common;
using DanmaobTisax.Domain.Auditing;

namespace DanmaobTisax.Domain.Identity;

public class Role : BaseEntity, ITenantOwned, IAuditable
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }

    protected Role()
    {
    }

    public Role(Guid tenantId, string name, string? description, bool isSystemRole)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
        }

        TenantId = tenantId;
        Name = name;
        Description = description;
        IsSystemRole = isSystemRole;
    }

    public void EnsureCanBeDeleted()
    {
        if (IsSystemRole)
        {
            throw new InvalidOperationException("System roles cannot be deleted.");
        }
    }
}
