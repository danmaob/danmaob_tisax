namespace DanmaobTisax.Application.Auditing;

public sealed class AuditLogQueryFilter
{
    public string? EntityName { get; init; }
    public string? EntityId { get; init; }
    public Guid? PerformedByUserId { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}