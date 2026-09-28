using System.Globalization;
using DanmaobTisax.Infrastructure.Localization;
using Microsoft.Extensions.Localization;
using Xunit;

namespace DanmaobTisax.Infrastructure.IntegrationTests.Localization;

public sealed class JsonStringLocalizerFactoryTests
{
    [Fact]
    public void Create_WithSpanishCulture_ResolvesKnownKeyFromSpanishFile()
    {
        var savedCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("es");

            var options = new SupportedCulturesOptions();
            var factory = new JsonStringLocalizerFactory(options);
            var localizer = factory.Create(typeof(JsonStringLocalizerFactoryTests));

            var result = localizer["Common.WelcomeMessage"];

            Assert.Equal("Bienvenido a DANMAOB TISAX Compliance Manager.", result.Value);
            Assert.False(result.ResourceNotFound);
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedCulture;
        }
    }

    [Fact]
    public void Create_WithEnglishCulture_ResolvesKnownKeyFromEnglishFile()
    {
        var savedCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("en");

            var options = new SupportedCulturesOptions();
            var factory = new JsonStringLocalizerFactory(options);
            var localizer = factory.Create(typeof(JsonStringLocalizerFactoryTests));

            var result = localizer["Common.WelcomeMessage"];

            Assert.Equal("Welcome to DANMAOB TISAX Compliance Manager.", result.Value);
            Assert.False(result.ResourceNotFound);
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedCulture;
        }
    }

    [Fact]
    public void Create_WithEnglishCulture_MissingKeyFallsBackToSpanishDefault()
    {
        var savedCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("en");

            var options = new SupportedCulturesOptions();
            var factory = new JsonStringLocalizerFactory(options);
            var localizer = factory.Create(typeof(JsonStringLocalizerFactoryTests));

            var result = localizer["Errors.OnlyInDefaultCulture"];

            Assert.Equal("Este texto solo existe en español, para probar el mecanismo de fallback.", result.Value);
            Assert.False(result.ResourceNotFound);
            Assert.Equal("es", result.SearchedLocation);
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedCulture;
        }
    }

    [Fact]
    public void Create_WithUnknownKey_ReturnsKeyItselfAndMarksResourceNotFound()
    {
        var savedCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("es");

            var options = new SupportedCulturesOptions();
            var factory = new JsonStringLocalizerFactory(options);
            var localizer = factory.Create(typeof(JsonStringLocalizerFactoryTests));

            var result = localizer["This.Key.Does.Not.Exist"];

            Assert.Equal("This.Key.Does.Not.Exist", result.Value);
            Assert.True(result.ResourceNotFound);
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedCulture;
        }
    }

    [Fact]
    public void Create_IndexerWithArguments_FormatsPlaceholdersUsingCurrentCulture()
    {
        var savedCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("es");

            var options = new SupportedCulturesOptions();
            var factory = new JsonStringLocalizerFactory(options);
            var localizer = factory.Create(typeof(JsonStringLocalizerFactoryTests));

            var result = localizer["Validation.Required", "Nombre"];

            Assert.Equal("El campo Nombre es obligatorio.", result.Value);
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedCulture;
        }
    }

    [Fact]
    public void Create_WithDefaultCultureResourceMissing_ThrowsInvalidOperationException()
    {
        var savedCulture = CultureInfo.CurrentUICulture;
        try
        {
            var options = new SupportedCulturesOptions
            {
                DefaultCulture = "fr",
                SupportedCultureCodes = new[] { "fr" }
            };

            Assert.Throws<InvalidOperationException>(() => new JsonStringLocalizerFactory(options));
        }
        finally
        {
            CultureInfo.CurrentUICulture = savedCulture;
        }
    }
}
