using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using DanmaobTisax.Api.Authorization;
using DanmaobTisax.Application.Common;
using DanmaobTisax.Application.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DanmaobTisax.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[RequirePermission("Identity.ManageUsers")]
public partial class UsersController : ControllerBase
{
    private readonly IUserAdministrationService _userAdministrationService;
    private readonly IStringLocalizer<UsersController> _localizer;

    public UsersController(IUserAdministrationService userAdministrationService, IStringLocalizer<UsersController> localizer)
    {
        _userAdministrationService = userAdministrationService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> ListUsers([FromQuery] bool? isActive, [FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var filter = new UserQueryFilter { IsActive = isActive, Search = search, PageNumber = pageNumber, PageSize = pageSize };
        var result = await _userAdministrationService.ListAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userAdministrationService.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userAdministrationService.CreateAsync(request.Email, request.FullName, request.Password, cancellationToken);
        if (result.Succeeded == true)
        {
            return CreatedAtAction(nameof(GetUserById), new { id = result.Value!.Id }, result.Value);
        }
        return MapFailure(result);
    }

    private ActionResult MapFailure(UserOperationResult result)
    {
        return result.Outcome switch
        {
            UserOperationOutcome.NotFound => NotFound(),
            UserOperationOutcome.EmailAlreadyExists => Conflict(BuildProblem(409, _localizer["Errors.UserEmailAlreadyExists"], "User.EmailAlreadyExists", null)),
            UserOperationOutcome.PasswordPolicyViolation => BadRequest(BuildProblem(400, _localizer["Errors.UserPasswordPolicyViolation"], "User.PasswordPolicyViolation", result.ViolatedRules)),
            UserOperationOutcome.InvalidStatusTransition => Conflict(BuildProblem(409, _localizer["Errors.UserInvalidStatusTransition"], "User.InvalidStatusTransition", null)),
            UserOperationOutcome.CannotDeactivateSelf => Conflict(BuildProblem(409, _localizer["Errors.UserCannotDeactivateSelf"], "User.CannotDeactivateSelf", null)),
            _ => throw new InvalidOperationException("Unexpected outcome: " + result.Outcome)
        };
    }

    private static ProblemDetails BuildProblem(int statusCode, string title, string errorCode, IReadOnlyList<string>? violatedRules)
    {
        var problem = new ProblemDetails { Status = statusCode, Title = title };
        problem.Extensions["errorCode"] = errorCode;
        if (violatedRules is not null)
        {
            problem.Extensions["violatedRules"] = violatedRules;
        }
        return problem;
    }

    public record CreateUserRequest([Required, EmailAddress, StringLength(256)] string Email, [Required, StringLength(200)] string FullName, [Required] string Password);

}
