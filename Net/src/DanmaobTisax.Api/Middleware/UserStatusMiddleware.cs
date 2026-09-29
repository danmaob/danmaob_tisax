using DanmaobTisax.Application.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DanmaobTisax.Api.Middleware;

public class UserStatusMiddleware
{
    private readonly RequestDelegate _next;

    public UserStatusMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserStatusEvaluator evaluator, IProblemDetailsService problemDetailsService, IStringLocalizer<UserStatusMiddleware> localizer)
    {
        if (context.User.Identity?.IsAuthenticated is not true)
        {
            await _next(context);
            return;
        }

        var tenantValue = context.User.FindFirst("tenant")?.Value;

        if (Guid.TryParse(tenantValue, out _) == false)
        {
            await _next(context);
            return;
        }

        var subValue = context.User.FindFirst("sub")?.Value;

        if (Guid.TryParse(subValue, out var userId) == false)
        {
            await _next(context);
            return;
        }

        var isBlocked = await evaluator.IsUserBlockedAsync(userId, context.RequestAborted);

        if (isBlocked == false)
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = localizer["Errors.UserInactive"].Value,
            Extensions = { ["errorCode"] = "User.Inactive" }
        };
        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext { HttpContext = context, ProblemDetails = problemDetails });
    }
}
