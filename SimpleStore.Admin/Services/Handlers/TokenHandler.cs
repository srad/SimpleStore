
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace Ansari.Frontend.Services.Handlers;

public class AccessTokenHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (accessor.HttpContext != null)
        {
            var accessToken = await accessor.HttpContext.GetTokenAsync("access_token");
            if (accessToken != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        // Continue down stream request.
        return await base.SendAsync(request, cancellationToken);
    }
}