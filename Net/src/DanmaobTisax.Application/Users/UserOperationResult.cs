namespace DanmaobTisax.Application.Users;

public sealed class UserOperationResult
{
    public UserOperationOutcome Outcome { get; }
    public UserDto? Value { get; }
    public IReadOnlyList<string>? ViolatedRules { get; }

    public UserOperationResult(UserOperationOutcome outcome, UserDto? value, IReadOnlyList<string>? violatedRules = null)
    {
        Outcome = outcome;
        Value = value;
        ViolatedRules = violatedRules;
    }

    public bool Succeeded => Outcome == UserOperationOutcome.Succeeded;
}
