using DanmaobTisax.Application.Identity;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobTisax.Infrastructure.Identity;

public class UserStatusEvaluator : IUserStatusEvaluator
{
    private readonly DanmaobTisaxDbContext _context;

    public UserStatusEvaluator(DanmaobTisaxDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsUserBlockedAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Users.AsNoTracking().AnyAsync(u => u.Id == userId && u.IsActive == false, cancellationToken);
    }
}
