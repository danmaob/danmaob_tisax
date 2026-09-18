namespace DanmaobTisax.Domain.Auditing;

/// <summary>
/// Represents an immutable audit log entry for tracking entity changes.
/// </summary>
public class AuditLog
{
    private AuditLog()
    {
    }

    public Guid Id { get; }

    public Guid? TenantId { get; }

    public string EntityName { get; } = string.Empty;

    public string EntityId { get; } = string.Empty;

    public AuditAction Action { get; }

    public Guid? PerformedByUserId { get; }

    public string? PerformedByDisplayName { get; }

    public DateTime PerformedAtUtc { get; }

    public string? OldValuesJson { get; }

    public string? NewValuesJson { get; }

    public string? ChangedColumnsJson { get; }

    public AuditLog(
        Guid? tenantId,
        string entityName,
        string entityId,
        AuditAction action,
        Guid? performedByUserId,
        string? performedByDisplayName,
        DateTime performedAtUtc,
        string? oldValuesJson,
        string? newValuesJson,
        string? changedColumnsJson)
    {
        if (string.IsNullOrWhiteSpace(entityName))
        {
            throw new ArgumentException("Entity name cannot be null or empty.", nameof(entityName));
        }

        if (string.IsNullOrWhiteSpace(entityId))
        {
            throw new ArgumentException("Entity id cannot be null or empty.", nameof(entityId));
        }

        Id = Guid.NewGuid();
        TenantId = tenantId;
        EntityName = entityName;
        EntityId = entityId;
        Action = action;
        PerformedByUserId = performedByUserId;
        PerformedByDisplayName = performedByDisplayName;
        PerformedAtUtc = performedAtUtc;
        OldValuesJson = oldValuesJson;
        NewValuesJson = newValuesJson;
        ChangedColumnsJson = changedColumnsJson;
    }
}
