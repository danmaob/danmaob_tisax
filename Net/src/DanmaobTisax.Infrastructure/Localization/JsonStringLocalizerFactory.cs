using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace DanmaobTisax.Infrastructure.Localization;

public sealed class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly JsonStringLocalizer _localizer;
    private readonly string _defaultCulture;

    public JsonStringLocalizerFactory(SupportedCulturesOptions options)
    {
        _defaultCulture = options.DefaultCulture;

        var resourcesByCulture = new Dictionary<string, IReadOnlyDictionary<string, string>>();

        foreach (var cultureCode in options.SupportedCultureCodes)
        {
            resourcesByCulture[cultureCode] = LoadResourcesForCulture(cultureCode);
        }

        var defaultResources = resourcesByCulture[_defaultCulture];

        if (defaultResources.Count == 0)
        {
            throw new InvalidOperationException(
                $"Embedded resource for the default culture '{_defaultCulture}' was not found in any of " +
                $"the available resource dictionaries. Available cultures: {string.Join(", ", resourcesByCulture.Keys)}");
        }

        _localizer = new JsonStringLocalizer(resourcesByCulture, _defaultCulture);
    }

    private static IReadOnlyDictionary<string, string> LoadResourcesForCulture(string cultureCode)
    {
        var resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
        var expectedResourceName = $"DanmaobTisax.Infrastructure.Localization.Resources.{cultureCode}.json";

        if (!resourceNames.Contains(expectedResourceName))
        {
            return new Dictionary<string, string>();
        }

        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(expectedResourceName);

        if (stream is null)
        {
            return new Dictionary<string, string>();
        }

        using var reader = new StreamReader(stream);
        var jsonContent = reader.ReadToEnd();

        if (string.IsNullOrWhiteSpace(jsonContent) || jsonContent.Trim() == "null")
        {
            return new Dictionary<string, string>();
        }

        return JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent) 
            ?? new Dictionary<string, string>();
    }

    public IStringLocalizer Create(Type resourceSource) => _localizer;

    public IStringLocalizer Create(string baseName, string location) => _localizer;
}
