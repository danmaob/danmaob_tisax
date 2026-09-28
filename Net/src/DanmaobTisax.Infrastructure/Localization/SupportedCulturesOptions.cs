using Microsoft.Extensions.Configuration;

namespace DanmaobTisax.Infrastructure.Localization;

public sealed class SupportedCulturesOptions
{
	public const string SectionName = "Localization";

	public string DefaultCulture { get; set; } = "es";

	public IReadOnlyList<string> SupportedCultureCodes { get; set; } = new List<string> { "es", "en" };

	public static SupportedCulturesOptions FromConfiguration(IConfiguration configuration)
	{
		var section = configuration.GetSection(SectionName);
		var options = new SupportedCulturesOptions();

		var defaultCultureValue = section.GetValue<string>(nameof(DefaultCulture));
		if (defaultCultureValue != null && !string.IsNullOrWhiteSpace(defaultCultureValue))
		{
			options.DefaultCulture = defaultCultureValue;
		}

		var culturesSection = section.GetSection(nameof(SupportedCultureCodes));
		var cultureValues = culturesSection.GetChildren()
			.Select(child => child.Value)
			.Where(value => !string.IsNullOrWhiteSpace(value))
			.Select(value => value!)
			.ToList();

		if (cultureValues.Count > 0)
		{
			options.SupportedCultureCodes = cultureValues;
		}

		return options;
	}
}
