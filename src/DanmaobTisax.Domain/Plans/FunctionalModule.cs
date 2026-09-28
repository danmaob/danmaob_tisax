using DanmaobTisax.Domain.Common;

namespace DanmaobTisax.Domain.Plans;

public class FunctionalModule : BaseEntity
{
    public string Code { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    protected FunctionalModule()
    {
    }

    public FunctionalModule(string code, int sortOrder)
    {
        if (string.IsNullOrEmpty(code.Trim()))
        {
            throw new ArgumentException("Module code is required.", nameof(code));
        }

        if (code.Length > 100)
        {
            throw new ArgumentException("Module code must not exceed 100 characters.", nameof(code));
        }

        if (sortOrder < 1)
        {
            throw new ArgumentException("Sort order must be at least 1.", nameof(sortOrder));
        }

        Code = code;
        SortOrder = sortOrder;
    }
}
