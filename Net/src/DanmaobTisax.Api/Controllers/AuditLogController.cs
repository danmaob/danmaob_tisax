using System;
using Asp.Versioning;
using DanmaobTisax.Api.Authorization;
using DanmaobTisax.Application.Auditing;
using DanmaobTisax.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace DanmaobTisax.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/audit-logs")]
[RequirePermission("Audit.Read")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogQueryService _auditLogQueryService;

    public AuditLogController(IAuditLogQueryService auditLogQueryService)
    {
        _auditLogQueryService = auditLogQueryService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAuditLogs(
        [FromQuery] string? entityName,
        [FromQuery] string? entityId,
        [FromQuery] Guid? performedByUserId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var filter = new AuditLogQueryFilter
        {
            EntityName = entityName,
            EntityId = entityId,
            PerformedByUserId = performedByUserId,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _auditLogQueryService.QueryAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("history/{entityName}/{entityId}")]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> GetHistoryForEntity(
        string entityName,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        var history = await _auditLogQueryService.GetHistoryForEntityAsync(entityName, entityId, cancellationToken);
        return Ok(history);
    }
}
