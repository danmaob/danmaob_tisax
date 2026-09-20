namespace DanmaobTisax.Application.Tenants;

public enum TenantOperationOutcome
{
    Succeeded,
    NotFound,
    InvalidName,
    NameAlreadyExists,
    InvalidStatusTransition
}
