namespace DanmaobTisax.Application.Identity;

public interface IPlatformAuthenticationService
{
    Task<LoginResult> LoginAsync(string email, string plainTextPassword, CancellationToken cancellationToken);
}
