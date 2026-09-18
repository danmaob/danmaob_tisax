namespace DanmaobTisax.Application.Interfaces;

public enum PasswordVerificationResult
{
    Failed,
    Success,
    SuccessRehashNeeded
}

public interface IPasswordHasher
{
    string Hash(string plainTextPassword);
    PasswordVerificationResult Verify(string hash, string plainTextPassword);
}
