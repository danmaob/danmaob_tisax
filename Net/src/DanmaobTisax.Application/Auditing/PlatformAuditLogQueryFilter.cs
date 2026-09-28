using DanmaobTisax.Domain.Auditing;

namespace DanmaobTisax.Application.Auditing;

public sealed class PlatformAuditLogQueryFilter
{
    public Guid? TenantId { get; init; }
    public string? EntityName { get; init; }
    public AuditAction? Action { get; init; }
    public Guid? PerformedByUserId { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
