using System.Text;
using DanmaobTisax.Api.ErrorHandling;
using DanmaobTisax.Domain.Tenants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DanmaobTisax.Infrastructure.IntegrationTests.ErrorHandling;

public sealed class GlobalExceptionHandlerDomainTests
{
    private static async Task<(int StatusCode, string Body)> HandleAsync(Exception exception)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();
        var provider = services.BuildServiceProvider();
        var handler = new GlobalExceptionHandler(provider.GetRequiredService<IProblemDetailsService>(), provider.GetRequiredService<ILogger<GlobalExceptionHandler>>());
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);
        Assert.True(handled);
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(httpContext.Response.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync();
        return (httpContext.Response.StatusCode, body);
    }

    private static void ThrowOutsideDomain()
    {
        throw new ArgumentException("internal-detail-must-not-leak");
    }

    [Fact]
    public async Task ArgumentExceptionThrownByDomainEntity_Returns400WithErrorCode()
    {
        var exception = Record.Exception(() => new Tenant(""));
        Assert.NotNull(exception);
        var (statusCode, body) = await HandleAsync(exception);
        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.Contains("Domain.ValidationFailed", body);
        Assert.Contains("Tenant name is required.", body);
    }

    [Fact]
    public async Task ArgumentExceptionThrownOutsideDomain_Returns500WithoutMessage()
    {
        var exception = Record.Exception(ThrowOutsideDomain);
        Assert.NotNull(exception);
        var (statusCode, body) = await HandleAsync(exception);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
        Assert.DoesNotContain("internal-detail-must-not-leak", body);
        Assert.DoesNotContain("Domain.ValidationFailed", body);
    }
}
