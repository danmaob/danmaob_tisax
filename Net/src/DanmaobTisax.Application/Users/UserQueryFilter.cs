namespace DanmaobTisax.Application.Users;

public sealed class UserQueryFilter
{
    public bool? IsActive { get; init; }
    public string? Search { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
