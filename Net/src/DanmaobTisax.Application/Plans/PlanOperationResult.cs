namespace DanmaobTisax.Application.Plans;

public sealed class PlanOperationResult
{
    public PlanOperationOutcome Outcome { get; }
    public PlanDto? Value { get; }

    public PlanOperationResult(PlanOperationOutcome outcome, PlanDto? value)
    {
        Outcome = outcome;
        Value = value;
    }

    public bool Succeeded => Outcome == PlanOperationOutcome.Succeeded;
}
