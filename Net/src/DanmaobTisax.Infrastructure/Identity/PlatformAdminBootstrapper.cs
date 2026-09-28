using DanmaobTisax.Application.Identity;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Infrastructure.Configuration;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DanmaobTisax.Infrastructure.Identity;

public static class PlatformAdminBootstrapper
{
    public static async Task RunAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DanmaobTisaxDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var passwordPolicyValidator = scope.ServiceProvider.GetRequiredService<IPasswordPolicyValidator>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        RequiredConfigurationValidator.EnsurePresent(configuration, "BootstrapPlatformAdmin:Email", "Set the environment variable BootstrapPlatformAdmin__Email.");
        RequiredConfigurationValidator.EnsurePresent(configuration, "BootstrapPlatformAdmin:Password", "Set the environment variable BootstrapPlatformAdmin__Password.");
        var email = configuration["BootstrapPlatformAdmin:Email"]!;
        var password = configuration["BootstrapPlatformAdmin:Password"]!;
        var fullName = configuration["BootstrapPlatformAdmin:FullName"];
        if (string.IsNullOrWhiteSpace(fullName) == true)
        {
            fullName = "Platform Administrator";
        }
        var alreadyExists = await context.PlatformAdministrators.AnyAsync(a => a.Email == email, cancellationToken);
        if (alreadyExists == true)
        {
            Console.WriteLine($"A platform administrator already exists with Email '{email}'. Skipping creation.");
            return;
        }
        var policyResult = passwordPolicyValidator.Validate(password);
        if (policyResult.IsValid == false)
        {
            Console.WriteLine("The platform administrator password does not meet the password policy. No platform administrator was created.");
            return;
        }
        var administrator = new PlatformAdministrator(email, passwordHasher.Hash(password), fullName);
        context.PlatformAdministrators.Add(administrator);
        await context.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"Platform administrator created successfully with email '{email}'.");
    }
}
