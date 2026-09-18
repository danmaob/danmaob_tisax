namespace DanmaobTisax.Domain.Tenants;

using DanmaobTisax.Domain.Common;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Required by EF Core for materialization.</summary>
    protected Tenant()
    {
    }

    public Tenant(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tenant name is required.", nameof(name));
        }

        Name = name;
    }
}
