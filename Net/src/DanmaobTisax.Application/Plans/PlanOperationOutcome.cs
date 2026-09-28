namespace DanmaobTisax.Application.Plans;

public enum PlanOperationOutcome
{
    Succeeded,
    NotFound,
    InvalidCode,
    InvalidName,
    CodeAlreadyExists,
    UnknownModuleCode,
    PlanInUse,
    DefaultPlanCannotBeDeactivated,
    InvalidStateTransition
}
