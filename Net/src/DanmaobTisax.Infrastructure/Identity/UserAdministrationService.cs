using DanmaobTisax.Application.Common;
using DanmaobTisax.Application.Identity;
using DanmaobTisax.Application.Interfaces;
using DanmaobTisax.Application.Users;
using DanmaobTisax.Domain.Identity;
using DanmaobTisax.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DanmaobTisax.Infrastructure.Identity;

public partial class UserAdministrationService
{
    private readonly DanmaobTisaxDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordPolicyValidator _passwordPolicyValidator;
    private readonly ICurrentTenantProvider _currentTenantProvider;
    private readonly ICurrentUserService _currentUserService;

    public UserAdministrationService(DanmaobTisaxDbContext context, IPasswordHasher passwordHasher, IPasswordPolicyValidator passwordPolicyValidator, ICurrentTenantProvider currentTenantProvider, ICurrentUserService currentUserService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _passwordPolicyValidator = passwordPolicyValidator;
        _currentTenantProvider = currentTenantProvider;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<UserDto>> ListAsync(UserQueryFilter filter, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(filter.PageSize, 1, 200);
        var pageNumber = Math.Max(filter.PageNumber, 1);
        var query = _context.Users.AsNoTracking();
        if (filter.IsActive.HasValue == true)
        {
            var isActiveValue = filter.IsActive.Value;
            query = query.Where(u => u.IsActive == isActiveValue);
        }
        var searchFragment = (filter.Search ?? string.Empty).Trim().ToLower();
        if (searchFragment.Length > 0)
        {
            query = query.Where(u => u.FullName.ToLower().Contains(searchFragment) || u.Email.ToLower().Contains(searchFragment));
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query.OrderBy(u => u.FullName).ThenBy(u => u.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var items = users.Select(ToDto).ToList();
        return new PagedResult<UserDto>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (user is null)
        {
            return null;
        }
        return ToDto(user);
    }

    public async Task<UserOperationResult> CreateAsync(string email, string fullName, string password, CancellationToken cancellationToken)
    {
        var policyResult = _passwordPolicyValidator.Validate(password);
        if (policyResult.IsValid == false)
        {
            return new UserOperationResult(UserOperationOutcome.PasswordPolicyViolation, null, policyResult.ViolatedRules);
        }
        var trimmedEmail = email.Trim();
        var lowerEmail = trimmedEmail.ToLower();
        var alreadyExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == lowerEmail, cancellationToken);
        if (alreadyExists == true)
        {
            return new UserOperationResult(UserOperationOutcome.EmailAlreadyExists, null);
        }
        var tenantId = _currentTenantProvider.CurrentTenantId;
        if (tenantId.HasValue == false)
        {
            throw new InvalidOperationException("No current tenant is available.");
        }
        var user = new User(tenantId.Value, trimmedEmail, _passwordHasher.Hash(password), fullName.Trim());
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return new UserOperationResult(UserOperationOutcome.Succeeded, ToDto(user));
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            IsActive = user.IsActive,
            LastLoginAtUtc = user.LastLoginAtUtc,
        };
    }
}
