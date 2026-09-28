using DanmaobTisax.Infrastructure.Security;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using Xunit;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Security;

public sealed class FieldEncryptionServiceTests
{
    [Fact]
    public void Protect_ThenUnprotect_ReturnsOriginalPlaintext()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var directoryInfo = new DirectoryInfo(tempPath);
        var provider = DataProtectionProvider.Create(directoryInfo);
        var service = new FieldEncryptionService(provider);

        var protectedValue = service.Protect("sensitive-value-123");
        var unprotectedValue = service.Unprotect(protectedValue);

        Assert.Equal("sensitive-value-123", unprotectedValue);
    }

    [Fact]
    public void Protect_ReturnsValueDifferentFromPlaintext()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var directoryInfo = new DirectoryInfo(tempPath);
        var provider = DataProtectionProvider.Create(directoryInfo);
        var service = new FieldEncryptionService(provider);

        var protectedValue = service.Protect("sensitive-value-123");

        Assert.NotEqual("sensitive-value-123", protectedValue);
    }

    [Fact]
    public void Unprotect_WithValueFromDifferentProvider_ThrowsCryptographicException()
    {
        var tempPath1 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var directoryInfo1 = new DirectoryInfo(tempPath1);
        var provider1 = DataProtectionProvider.Create(directoryInfo1);
        var service1 = new FieldEncryptionService(provider1);

        var tempPath2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var directoryInfo2 = new DirectoryInfo(tempPath2);
        var provider2 = DataProtectionProvider.Create(directoryInfo2);
        var service2 = new FieldEncryptionService(provider2);

        var protectedValue = service1.Protect("sensitive-value-123");

        Assert.Throws<CryptographicException>(() => service2.Unprotect(protectedValue));
    }
}
