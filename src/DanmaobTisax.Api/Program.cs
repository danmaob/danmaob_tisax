using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DanmaobTisax.Application;
using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Infrastructure;
using DanmaobTisax.Infrastructure.Identity;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using DanmaobTisax.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

RequiredConfigurationValidator.EnsurePresent(
    builder.Configuration,
    "ConnectionStrings:DefaultConnection",
    "In Development, set it with: dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<value>\" " +
    "--project src/DanmaobTisax.Api. In deployed environments, set the environment variable " +
    "ConnectionStrings__DefaultConnection.");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
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
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
