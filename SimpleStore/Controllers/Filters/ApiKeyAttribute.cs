using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SimpleStore.Controllers.Filters;

public class ApiKeyAttribute : ServiceFilterAttribute
{
    public ApiKeyAttribute() : base(typeof(IAuthorizationFilter))
    {
    }
}