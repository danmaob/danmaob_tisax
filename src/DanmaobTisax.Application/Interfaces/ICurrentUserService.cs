namespace DanmaobTisax.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? DisplayName { get; }
}