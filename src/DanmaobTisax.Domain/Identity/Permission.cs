using DanmaobTisax.Domain.Common;

namespace DanmaobTisax.Domain.Identity;

public class Permission : BaseEntity
{
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }

    protected Permission()
    {
    }

    public Permission(string module, string action, string? description)
    {
        if (string.IsNullOrWhiteSpace(module))
        {
            throw new ArgumentException("Module cannot be null or whitespace");
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("Action cannot be null or whitespace");
        }

        Module = module;
        Action = action;
        Description = description;
    }

    public string Code => Module + "." + Action;
}
