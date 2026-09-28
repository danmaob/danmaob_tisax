using Microsoft.AspNetCore.Authorization;

namespace DanmaobTisax.Api.Authorization;

public sealed class RequireModuleAttribute : AuthorizeAttribute
{
    public RequireModuleAttribute(string moduleCode)
    {
        Policy = "Module:" + moduleCode;
    }
}
