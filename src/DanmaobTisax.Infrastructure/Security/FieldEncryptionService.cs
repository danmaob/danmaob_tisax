using DanmaobTisax.Application.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace DanmaobTisax.Infrastructure.Security;

public sealed class FieldEncryptionService : IFieldEncryptionService
{
    private readonly IDataProtector _protector;

    public FieldEncryptionService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector("DanmaobTisax.FieldEncryption");
    }

    public string Protect(string plaintext)
    {
        return _protector.Protect(plaintext);
    }

    public string Unprotect(string protectedValue)
    {
        return _protector.Unprotect(protectedValue);
    }
}
