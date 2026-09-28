using DanmaobTisax.Api.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using Xunit;

namespace DanmaobTisax.Infrastructure.IntegrationTests.ErrorHandling;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_WritesSanitizedProblemDetails_AndOmitsExceptionMessage()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();
        var provider = services.BuildServiceProvider();

        var problemDetailsService = provider.GetRequiredService<IProblemDetailsService>();
        var logger = provider.GetRequiredService<ILogger<GlobalExceptionHandler>>();
        var handler = new GlobalExceptionHandler(problemDetailsService, logger);

        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var exception = new InvalidOperationException("connection failed: Server=secret-internal-host;Password=super-secret-password");

        var handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(httpContext.Response.Body, Encoding.UTF8);
        var responseText = await reader.ReadToEndAsync();

        Assert.Contains("An unexpected error occurred.", responseText);
        Assert.DoesNotContain("secret-internal-host", responseText);
        Assert.DoesNotContain("super-secret-password", responseText);
    }
}
