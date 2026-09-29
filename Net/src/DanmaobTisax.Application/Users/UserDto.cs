namespace DanmaobTisax.Application.Users;

public sealed class UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime? LastLoginAtUtc { get; init; }
}
