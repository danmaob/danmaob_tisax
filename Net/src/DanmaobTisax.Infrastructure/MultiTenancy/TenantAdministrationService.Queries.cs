using DanmaobTisax.Application.Common;
using DanmaobTisax.Application.Tenants;
using Microsoft.EntityFrameworkCore;

namespace DanmaobTisax.Infrastructure.MultiTenancy;

public partial class TenantAdministrationService
{
    public async Task<PagedResult<TenantDto>> ListAsync(TenantQueryFilter filter, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);
        var pageNumber = Math.Max(filter.PageNumber, 1);
        var query = _context.Tenants.AsNoTracking();
        if (filter.Status.HasValue == true)
        {
            var statusValue = filter.Status.Value;
            query = query.Where(t => t.Status == statusValue);
        }
        var nameFragment = (filter.Name ?? string.Empty).Trim().ToLower();
        if (nameFragment.Length > 0)
        {
            query = query.Where(t => t.Name.ToLower().Contains(nameFragment));
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var tenants = await query.OrderBy(t => t.Name).ThenBy(t => t.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var items = tenants.Select(ToDto).ToList();
        return new PagedResult<TenantDto>(items, totalCount, pageNumber, pageSize);
    }
}
