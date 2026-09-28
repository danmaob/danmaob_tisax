using DanmaobTisax.Application.Common;
using DanmaobTisax.Application.Tenants;
using DanmaobTisax.Domain.Tenants;
using Microsoft.AspNetCore.Mvc;

namespace DanmaobTisax.Api.Controllers;

public partial class TenantsController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<TenantDto>>> ListTenants([FromQuery] TenantStatus? status, [FromQuery] string? name, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var filter = new TenantQueryFilter { Status = status, Name = name, PageNumber = pageNumber, PageSize = pageSize };

        var result = await _tenantAdministrationService.ListAsync(filter, cancellationToken);

        return Ok(result);
    }
}
