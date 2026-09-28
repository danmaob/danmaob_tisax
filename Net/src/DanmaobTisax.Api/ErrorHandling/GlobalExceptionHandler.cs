using DanmaobTisax.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DanmaobTisax.Api.ErrorHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ArgumentException && exception.TargetSite?.DeclaringType?.Assembly == typeof(DomainAssemblyMarker).Assembly)
        {
            _logger.LogWarning(exception, "Business rule violation processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            var problem = new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "The request violates a business rule.", Detail = exception.Message };
            problem.Extensions["errorCode"] = "Domain.ValidationFailed";
            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = problem });
        }

        _logger.LogError(
            exception,
            "Unhandled exception processing {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var result = await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "An unexpected error occurred while processing the request. Contact support if the problem persists."
            }
        });

        return result;
    }
}
