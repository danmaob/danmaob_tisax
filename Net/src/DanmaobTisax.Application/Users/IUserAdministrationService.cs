using DanmaobTisax.Application.Common;

namespace DanmaobTisax.Application.Users;

public interface IUserAdministrationService
{
    Task<PagedResult<UserDto>> ListAsync(UserQueryFilter filter, CancellationToken cancellationToken);
    Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<UserOperationResult> CreateAsync(string email, string fullName, string password, CancellationToken cancellationToken);
    Task<UserOperationResult> RenameAsync(Guid id, string fullName, CancellationToken cancellationToken);
    Task<UserOperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken);
    Task<UserOperationResult> ReactivateAsync(Guid id, CancellationToken cancellationToken);
}
