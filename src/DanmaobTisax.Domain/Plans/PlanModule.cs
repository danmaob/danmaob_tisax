namespace DanmaobTisax.Domain.Plans;

using DanmaobTisax.Domain.Common;
using DanmaobTisax.Domain.Auditing;

public class PlanModule : BaseEntity, IAuditable
{
    public Guid PlanId { get; private set; }
    
    public string ModuleCode { get; private set; } = string.Empty;
    
    public bool IsEnabled { get; private set; } = false;

    protected PlanModule()
    {
    }

    public PlanModule(Guid planId, string moduleCode, bool isEnabled)
    {
        if (planId == Guid.Empty)
        {
            throw new ArgumentException("Plan id is required.", nameof(planId));
        }

        if (string.IsNullOrWhiteSpace(moduleCode))
        {
            throw new ArgumentException("Module code is required.", nameof(moduleCode));
        }

        if (moduleCode.Length > 100)
        {
            throw new ArgumentException("Module code must not exceed 100 characters.", nameof(moduleCode));
        }

        PlanId = planId;
        ModuleCode = moduleCode;
        IsEnabled = isEnabled;
    }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }
}
