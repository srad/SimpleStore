using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SimpleStore.Controllers.Filters;

public class ApiAuthorizationFilter : IAuthorizationFilter
{
    private const string ApiKeyHeaderName = "X-API-Key";

    private readonly IApiKeyValidator _apiKeyValidator;

    public ApiAuthorizationFilter(IApiKeyValidator apiKeyValidator)
    {
        _apiKeyValidator = apiKeyValidator;
    }

    public async void OnAuthorization(AuthorizationFilterContext context)
    {
        string apiKey = context.HttpContext.Request.Headers[ApiKeyHeaderName];

        if (!await _apiKeyValidator.IsValid(apiKey))
        {
            context.Result = new UnauthorizedResult();
        }
    }
}