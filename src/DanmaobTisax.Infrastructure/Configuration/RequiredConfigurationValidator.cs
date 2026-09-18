using Microsoft.Extensions.Configuration;

namespace DanmaobTisax.Infrastructure.Configuration;

/// <summary>
/// Fails fast, with a clear message and without ever including the actual
/// value, when a required configuration key is missing. Generic on
/// purpose — reused for the connection string here and intended for any
/// future required secret (e.g. TS-00-3's JWT signing key). See ADR-0003.
/// </summary>
public static class RequiredConfigurationValidator
{
    public static void EnsurePresent(IConfiguration configuration, string key, string guidance)
    {
        var value = configuration[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Missing required configuration key: '{key}'. {guidance} " +
                "This message intentionally does not include any configuration value.");
        }
    }
}
