using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using DanmaobTisax.Application;
using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Infrastructure;
using DanmaobTisax.Infrastructure.Identity;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using DanmaobTisax.Infrastructure.Configuration;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Scalar.AspNetCore;
using DanmaobTisax.Infrastructure.Localization;

var builder = WebApplication.CreateBuilder(args);

RequiredConfigurationValidator.EnsurePresent(
    builder.Configuration,
    "ConnectionStrings:DefaultConnection",
    "In Development, set it with: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<value>\" " +
    "--project src/DanmaobTisax.Api. In deployed environments, set the environment variable " +
    "ConnectionStrings__DefaultConnection.");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddLocalizationServices(builder.Configuration);

var supportedCulturesOptions = SupportedCulturesOptions.FromConfiguration(builder.Configuration);
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(supportedCulturesOptions.DefaultCulture);
    options.SupportedCultures = supportedCulturesOptions.SupportedCultureCodes
        .Select(c => CultureInfo.GetCultureInfo(c))
        .ToList();
    options.SupportedUICultures = options.SupportedCultures;
});

builder.Services.AddHealthChecks();
builder.Services.AddHttpContextAccessor();

RequiredConfigurationValidator.EnsurePresent(
    builder.Configuration,
    "Jwt:Issuer",
    "Set it in configuration, e.g. via dotnet user-secrets set \"Jwt:Issuer\" \"<value>\".");
RequiredConfigurationValidator.EnsurePresent(
    builder.Configuration,
    "Jwt:Audience",
    "Set it in configuration, e.g. via dotnet user-secrets set \"Jwt:Audience\" \"<value>\".");
RequiredConfigurationValidator.EnsurePresent(
    builder.Configuration,
    "Jwt:SigningKey",
    "Set it in configuration, e.g. via dotnet user-secrets set \"Jwt:SigningKey\" \"<value>\".");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
})
.AddOpenApi();

var app = builder.Build();

if (args.Contains("--bootstrap-admin"))
{
    await AdminBootstrapper.RunAsync(app.Services, default);
    return;
}

using (var startupScope = app.Services.CreateScope())
{
    var scopeContext = startupScope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
    await scopeContext.Database.EnsureCreatedAsync();
    await PermissionCatalogSeeder.SeedAsync(scopeContext, CancellationToken.None);
}

// Configure the HTTP request pipeline.
app.UseRequestLocalization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().WithDocumentPerVersion();

    var versions = app.DescribeApiVersions();
    app.MapScalarApiReference(v => {
        for (int i = 0; i < versions.Count; i++)
        {
            v.AddDocument(versions[i].GroupName, $"{versions[i].ApiVersion}", isDefault: i == versions.Count - 1);
        }
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
