namespace DanmaobTisax.Application.Tenants;

public sealed class TenantOperationResult
{
    public TenantOperationOutcome Outcome { get; }
    public TenantDto? Value { get; }

    public TenantOperationResult(TenantOperationOutcome outcome, TenantDto? value)
    {
        Outcome = outcome;
        Value = value;
    }

    public bool Succeeded => Outcome == TenantOperationOutcome.Succeeded;
}
