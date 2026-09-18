using DanmaobTisax.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace DanmaobTisax.Infrastructure.Identity;

public class PasswordHasherService : IPasswordHasher
{
    private const string AlgorithmTag = "pbkdf2-sha256";
    private const int CurrentIterations = 210_000;
    private const int DerivedKeyLength = 32;
    private const int SaltLength = 16;

    public string Hash(string plainTextPassword)
    {
        if (string.IsNullOrEmpty(plainTextPassword))
        {
            throw new ArgumentNullException(nameof(plainTextPassword));
        }

        var salt = RandomNumberGenerator.GetBytes(SaltLength);
        var derivedBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(plainTextPassword),
            salt,
            CurrentIterations,
            HashAlgorithmName.SHA256,
            DerivedKeyLength);

        var saltBase64 = Convert.ToBase64String(salt);
        var hashBase64 = Convert.ToBase64String(derivedBytes);

        return $"{AlgorithmTag}${CurrentIterations}${saltBase64}${hashBase64}";
    }

    public PasswordVerificationResult Verify(string hash, string plainTextPassword)
    {
        if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(plainTextPassword))
        {
            return PasswordVerificationResult.Failed;
        }

        // Parse the hash format: pbkdf2-sha256$iterations$salt$hash
        var parts = hash.Split('$');
        if (parts.Length != 4 || parts[0] != AlgorithmTag || !int.TryParse(parts[1], out var storedIterations))
        {
            return PasswordVerificationResult.Failed;
        }

        try
        {
            var storedSalt = Convert.FromBase64String(parts[2]);
            var storedHash = Convert.FromBase64String(parts[3]);

            var derivedBytes = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(plainTextPassword),
                storedSalt,
                storedIterations,
                HashAlgorithmName.SHA256,
                storedHash.Length);

            if (!CompareBytes(storedHash, derivedBytes))
            {
                return PasswordVerificationResult.Failed;
            }

            return storedIterations < CurrentIterations
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Success;
        }
        catch (FormatException)
        {
            // Invalid hash format (bad base64).
            return PasswordVerificationResult.Failed;
        }
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
