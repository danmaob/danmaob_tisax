namespace DanmaobTisax.Application.Tenants;

public enum TenantOperationOutcome
{
    Succeeded,
    NotFound,
    InvalidName,
    NameAlreadyExists,
    InvalidStatusTransition,
	/// <summary>The referenced plan was not found.</summary>
	PlanNotFound,
	/// <summary>The referenced plan is inactive.</summary>
	PlanInactive
}
