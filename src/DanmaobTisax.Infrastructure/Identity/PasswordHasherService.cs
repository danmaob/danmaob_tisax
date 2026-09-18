using DanmaobTisax.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace DanmaobTisax.Infrastructure.Identity;

#nullable enable
#pragma warning disable SYSLIB0060 // Rfc2898DeriveBytes constructor is obsolete but still functional

public class PasswordHasherService : IPasswordHasher
{
    private const int MinCost = 10;
    
    public PasswordHasherService()
    {
    }

    public string Hash(string plainTextPassword)
    {
        if (string.IsNullOrEmpty(plainTextPassword))
        {
            throw new ArgumentNullException(nameof(plainTextPassword));
        }

        // Generate a random salt
        byte[] salt = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        // Derive key with PBKDF2 using constructor (ignoring deprecation warning)
        byte[] derivedBytes = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(plainTextPassword), salt, MinCost).GetBytes(32);
        
        // Base64 encode for storage
        string saltBase64 = Convert.ToBase64String(salt);
        string hashBase64 = Convert.ToBase64String(derivedBytes);
        
        return $"2a${saltBase64}${hashBase64}";
    }

    public DanmaobTisax.Application.Interfaces.PasswordVerificationResult Verify(string hash, string plainTextPassword)
    {
        if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(plainTextPassword))
        {
            return DanmaobTisax.Application.Interfaces.PasswordVerificationResult.Failed;
        }

        // Parse the hash format: 2a$salt$hash
        var parts = hash.Split('$');
        if (parts.Length < 3)
        {
            return DanmaobTisax.Application.Interfaces.PasswordVerificationResult.Failed;
        }

        try
        {
            string saltBase64 = parts[1];
            string hashBase64 = parts[2];

            byte[] storedSalt = Convert.FromBase64String(saltBase64);
            byte[] storedHash = Convert.FromBase64String(hashBase64);

            // Re-derive key from password and same salt
            byte[] derivedBytes = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(plainTextPassword), storedSalt, MinCost).GetBytes(storedHash.Length);

            // Compare bytes safely to avoid timing attacks
            if (CompareBytes(storedHash, derivedBytes))
            {
                return DanmaobTisax.Application.Interfaces.PasswordVerificationResult.Success;
            }
        }
        catch (Exception)
        {
            // Invalid hash format or derivation failed
        }

        return DanmaobTisax.Application.Interfaces.PasswordVerificationResult.Failed;
    }

    private static bool CompareBytes(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;

        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }

}
