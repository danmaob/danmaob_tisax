namespace DanmaobTisax.Application.Tenants;

public sealed class TenantModuleOperationResult
{
    public TenantModuleOperationOutcome Outcome { get; }

    public TenantModuleStateDto? Value { get; }

    public TenantModuleOperationResult(TenantModuleOperationOutcome outcome, TenantModuleStateDto? value)
    {
        Outcome = outcome;
        Value = value;
    }

    public bool Succeeded => Outcome == TenantModuleOperationOutcome.Succeeded;
}
