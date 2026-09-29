namespace DanmaobTisax.Application.Users;

public enum UserOperationOutcome
{
    Succeeded,
    NotFound,
    EmailAlreadyExists,
    PasswordPolicyViolation,
    InvalidStatusTransition,
    CannotDeactivateSelf
}
