using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace DanmaobTisax.Infrastructure.Localization;

public static class LocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddLocalizationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var options = SupportedCulturesOptions.FromConfiguration(configuration);
        services.AddSingleton(options);
        services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
        services.AddSingleton(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

        return services;
    }
}
