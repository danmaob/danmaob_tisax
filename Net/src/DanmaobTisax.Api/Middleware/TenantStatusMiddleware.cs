using DanmaobTisax.Application.Tenants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DanmaobTisax.Api.Middleware;

public class TenantStatusMiddleware
{
    private readonly RequestDelegate _next;

    public TenantStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantStatusEvaluator evaluator,
        IProblemDetailsService problemDetailsService,
        IStringLocalizer<TenantStatusMiddleware> localizer)
    {
        if (context.User.Identity?.IsAuthenticated is not true)
        {
            await _next(context);
            return;
        }

        var tenantValue = context.User.FindFirst("tenant")?.Value;
        if (!Guid.TryParse(tenantValue, out var tenantId))
        {
            await _next(context);
            return;
        }

        var isBlocked = await evaluator.IsTenantBlockedAsync(tenantId, context.RequestAborted);

        if (!isBlocked)
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = localizer["Errors.TenantNotActive"].Value,
            Extensions = { ["errorCode"] = "Tenant.NotActive" }
        };

        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problemDetails
        });
    }
}
