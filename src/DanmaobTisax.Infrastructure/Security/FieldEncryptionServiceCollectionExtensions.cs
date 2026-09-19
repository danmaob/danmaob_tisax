using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Infrastructure.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace DanmaobTisax.Infrastructure.Configuration
{
    public static class FieldEncryptionServiceCollectionExtensions
    {
        public static IServiceCollection AddFieldEncryption(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            string? keysDirectory = configuration["DataProtection:KeysDirectory"];
            
            if (!string.IsNullOrWhiteSpace(keysDirectory))
            {
                // Use configured path
            }
            else
            {
                keysDirectory = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "App_Data",
                    "dataprotection-keys"
                );
            }

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory!))
                .SetApplicationName("DanmaobTisax");

            services.AddSingleton<IFieldEncryptionService, FieldEncryptionService>();

            return services;
        }
    }
}
