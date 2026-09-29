using System.ComponentModel.DataAnnotations;
using DanmaobTisax.Application.Users;
using Microsoft.AspNetCore.Mvc;

namespace DanmaobTisax.Api.Controllers;

public partial class UsersController
{
    [HttpPut("{id:guid}/name")]
    public async Task<ActionResult<UserDto>> RenameUser(Guid id, [FromBody] RenameUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userAdministrationService.RenameAsync(id, request.FullName, cancellationToken);
        if (result.Succeeded == true)
        {
            return Ok(result.Value);
        }
        return MapFailure(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<UserDto>> DeactivateUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await _userAdministrationService.DeactivateAsync(id, cancellationToken);
        if (result.Succeeded == true)
        {
            return Ok(result.Value);
        }
        return MapFailure(result);
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<ActionResult<UserDto>> ReactivateUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await _userAdministrationService.ReactivateAsync(id, cancellationToken);
        if (result.Succeeded == true)
        {
            return Ok(result.Value);
        }
        return MapFailure(result);
    }

    public record RenameUserRequest([Required, StringLength(200)] string FullName);
}
