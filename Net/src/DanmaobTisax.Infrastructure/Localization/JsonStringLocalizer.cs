using System.Globalization;
using Microsoft.Extensions.Localization;

namespace DanmaobTisax.Infrastructure.Localization;

internal sealed class JsonStringLocalizer : IStringLocalizer
{
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _resourcesByCulture;
    private readonly string _defaultCulture;

    internal JsonStringLocalizer(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> resourcesByCulture,
        string defaultCulture)
    {
        _resourcesByCulture = resourcesByCulture;
        _defaultCulture = defaultCulture;
    }

    private (string? Value, bool Found, string SearchedCulture) Resolve(string name)
    {
        var currentCulture = CultureInfo.CurrentUICulture.Name;

        if (_resourcesByCulture.TryGetValue(currentCulture, out var cultureResources) &&
            cultureResources.TryGetValue(name, out var value))
        {
            return (value, true, currentCulture);
        }

        if (_resourcesByCulture.TryGetValue(_defaultCulture, out var defaultResources) &&
            defaultResources.TryGetValue(name, out value))
        {
            return (value, true, _defaultCulture);
        }

        return (null, false, currentCulture);
    }

    public LocalizedString this[string name]
    {
        get
        {
            var (value, found, searchedCulture) = Resolve(name);
            return new LocalizedString(name, value ?? name, !found, searchedCulture);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var (value, found, searchedCulture) = Resolve(name);
            var format = !found ? name : value!;
            var formattedValue = string.Format(CultureInfo.CurrentUICulture, format, arguments);
            return new LocalizedString(name, formattedValue, !found, searchedCulture);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var currentCulture = CultureInfo.CurrentUICulture.Name;
        var keys = new HashSet<string>();

        if (_resourcesByCulture.TryGetValue(currentCulture, out var currentResources))
        {
            foreach (var key in currentResources.Keys)
            {
                keys.Add(key);
            }
        }

        if (includeParentCultures && _resourcesByCulture.TryGetValue(_defaultCulture, out var defaultResources))
        {
            foreach (var key in defaultResources.Keys)
            {
                keys.Add(key);
            }
        }

        foreach (var key in keys)
        {
            var (value, found, searchedCulture) = Resolve(key);
            yield return new LocalizedString(key, value ?? key, !found, searchedCulture);
        }
    }
}
