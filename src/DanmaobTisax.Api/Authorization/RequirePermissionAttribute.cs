using Microsoft.AspNetCore.Authorization;

namespace DanmaobTisax.Api.Authorization;

public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permissionCode)
    {
        Policy = "Permission:" + permissionCode;
    }
}
