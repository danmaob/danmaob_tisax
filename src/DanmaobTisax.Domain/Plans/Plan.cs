using DanmaobTisax.Domain.Common;
using DanmaobTisax.Domain.Auditing;
using DanmaobTisax.Domain.Exceptions;
using System;

namespace DanmaobTisax.Domain.Plans;

public class Plan : BaseEntity, IAuditable
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    protected Plan()
    {
    }

    public Plan(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Plan code is required.", nameof(code));
        }

        if (code.Trim().Length > 50)
        {
            throw new ArgumentException("Plan code must not exceed 50 characters.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Plan name is required.", nameof(name));
        }

        if (name.Trim().Length > 100)
        {
            throw new ArgumentException("Plan name must not exceed 100 characters.", nameof(name));
        }

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Plan name is required.", nameof(name));
        }

        if (name.Trim().Length > 100)
        {
            throw new ArgumentException("Plan name must not exceed 100 characters.", nameof(name));
        }

        Name = name.Trim();
    }

    public void Deactivate()
    {
        if (Id == PlanCatalog.FreePlanId)
        {
            throw new InvalidPlanStateException("The default plan cannot be deactivated.");
        }

        if (IsActive == false)
        {
            throw new InvalidPlanStateException("The plan is already inactive.");
        }

        IsActive = false;
    }

    public void Reactivate()
    {
        if (IsActive == true)
        {
            throw new InvalidPlanStateException("The plan is already active.");
        }

        IsActive = true;
    }
}
