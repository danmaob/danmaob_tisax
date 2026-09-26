using System.Text;
using Asp.Versioning;
using DanmaobTisax.Api.Auditing;
using DanmaobTisax.Api.Authorization;
using DanmaobTisax.Application.Auditing;
using DanmaobTisax.Application.Common;
using DanmaobTisax.Domain.Auditing;
using Microsoft.AspNetCore.Mvc;

namespace DanmaobTisax.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/platform/audit")]
[RequirePermission("Platform.ReadAudit")]
public class PlatformAuditController : ControllerBase
{
    private readonly IPlatformAuditQueryService _platformAuditQueryService;

    public PlatformAuditController(IPlatformAuditQueryService platformAuditQueryService)
    {
        _platformAuditQueryService = platformAuditQueryService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PlatformAuditLogDto>>> GetPlatformAudit(
        [FromQuery] Guid? tenantId,
        [FromQuery] string? entityName,
        [FromQuery] AuditAction? action,
        [FromQuery] Guid? performedByUserId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var filter = new PlatformAuditLogQueryFilter
        {
            TenantId = tenantId,
            EntityName = entityName,
            Action = action,
            PerformedByUserId = performedByUserId,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _platformAuditQueryService.QueryAsync(filter, cancellationToken);

        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportPlatformAudit(
        [FromQuery] Guid? tenantId,
        [FromQuery] string? entityName,
        [FromQuery] AuditAction? action,
        [FromQuery] Guid? performedByUserId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken cancellationToken = default)
    {
        var filter = new PlatformAuditLogQueryFilter
        {
            TenantId = tenantId,
            EntityName = entityName,
            Action = action,
            PerformedByUserId = performedByUserId,
            FromUtc = fromUtc,
            ToUtc = toUtc
        };

        var rows = await _platformAuditQueryService.ExportAsync(filter, cancellationToken);

        var csv = PlatformAuditCsv.Build(rows);

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();

        return File(bytes, "text/csv", "platform-audit.csv");
    }
}
