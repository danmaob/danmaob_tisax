using DanmaobTisax.Domain.Tenants;

namespace DanmaobTisax.Domain.Exceptions;

public class InvalidTenantStatusTransitionException : InvalidOperationException
{
    public TenantStatus CurrentStatus { get; }

    public InvalidTenantStatusTransitionException(TenantStatus currentStatus, string attemptedAction)
        : base($"Cannot {attemptedAction} a tenant whose status is {currentStatus}.")
    {
        CurrentStatus = currentStatus;
    }
}
