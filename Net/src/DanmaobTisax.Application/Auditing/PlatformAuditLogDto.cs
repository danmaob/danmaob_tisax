public sealed class PlatformAuditLogDto
{
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? AffectedTenantId { get; init; }
    public string EntityName { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public Guid? PerformedByUserId { get; init; }
    public string? PerformedByDisplayName { get; init; }
    public DateTime PerformedAtUtc { get; init; }
    public string? OldValuesJson { get; init; }
    public string? NewValuesJson { get; init; }
    public string? ChangedColumnsJson { get; init; }
}
