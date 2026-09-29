using DanmaobTisax.Application.Users;
using Microsoft.EntityFrameworkCore;

namespace DanmaobTisax.Infrastructure.Identity;

public partial class UserAdministrationService : IUserAdministrationService
{
    public async Task<UserOperationResult> RenameAsync(Guid id, string fullName, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return new UserOperationResult(UserOperationOutcome.NotFound, null);
        }
        user.Rename(fullName.Trim());
        await _context.SaveChangesAsync(cancellationToken);
        return new UserOperationResult(UserOperationOutcome.Succeeded, ToDto(user));
    }

    public async Task<UserOperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return new UserOperationResult(UserOperationOutcome.NotFound, null);
        }
        if (_currentUserService.UserId == id)
        {
            return new UserOperationResult(UserOperationOutcome.CannotDeactivateSelf, null);
        }
        if (user.IsActive == false)
        {
            return new UserOperationResult(UserOperationOutcome.InvalidStatusTransition, null);
        }
        user.Deactivate();
        var now = DateTime.UtcNow;
        var activeTokens = await _context.RefreshTokens.Where(t => t.UserId == id && t.RevokedAtUtc == null && t.ExpiresAtUtc > now).ToListAsync(cancellationToken);
        foreach (var token in activeTokens)
        {
            token.Revoke();
        }
        await _context.SaveChangesAsync(cancellationToken);
        return new UserOperationResult(UserOperationOutcome.Succeeded, ToDto(user));
    }

    public async Task<UserOperationResult> ReactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return new UserOperationResult(UserOperationOutcome.NotFound, null);
        }
        if (user.IsActive == true)
        {
            return new UserOperationResult(UserOperationOutcome.InvalidStatusTransition, null);
        }
        user.Reactivate();
        await _context.SaveChangesAsync(cancellationToken);
        return new UserOperationResult(UserOperationOutcome.Succeeded, ToDto(user));
    }
}
