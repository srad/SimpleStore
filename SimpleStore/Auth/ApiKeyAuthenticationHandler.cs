using System.Net.Mime;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using SimpleStore.Models;

namespace SimpleStore.Auth;

/// <summary>
/// See: https://stackoverflow.com/questions/72966528/can-api-key-and-jwt-token-be-used-in-the-same-net-6-webapi
/// </summary>
/// <param name="factory"></param>
/// <param name="options"></param>
/// <param name="loggerFactory"></param>
/// <param name="logger"></param>
/// <param name="encoder"></param>
/// <param name="clock"></param>
public class ApiKeyAuthenticationHandler(
    IDbContextFactory<ApplicationDbContext> factory,
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory loggerFactory,
    ILogger<ApiKeyAuthenticationHandler> logger,
    UrlEncoder encoder,
    ISystemClock clock) : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, loggerFactory, encoder, clock)
{
    private enum AuthenticationFailureReason
    {
        NONE = 0,
        API_KEY_HEADER_NOT_PROVIDED,
        API_KEY_HEADER_VALUE_NULL,
        API_KEY_INVALID
    }

    private readonly ILogger _logger = logger;

    private AuthenticationFailureReason _failureReason = AuthenticationFailureReason.NONE;

    const string ApiKeyHeaderName = "X-API-Key";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        //ApiKey header get
        if (!TryGetApiKeyHeader(out string providedApiKey, out AuthenticateResult authenticateResult))
        {
            return authenticateResult;
        }

        // Add the role claim, also for the API auth.
        var key = await ApiKeyCheckAsync(providedApiKey);
        if (key != null)
        {
            var principal = new ClaimsPrincipal(); 
            
            var myIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, "objectstore"),
                new Claim(ClaimTypes.Name, key.Title),
            });
            principal.AddIdentity(myIdentity);

            var ticket = new AuthenticationTicket(principal, ApiKeyAuthenticationOptions.Scheme);

            return AuthenticateResult.Success(ticket);
        }

        _failureReason = AuthenticationFailureReason.API_KEY_INVALID;
        return AuthenticateResult.NoResult();
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        //Create response
        Response.Headers.Append(HeaderNames.WWWAuthenticate, $@"Authorization realm=""{ApiKeyAuthenticationOptions.DefaultScheme}""");
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.ContentType = MediaTypeNames.Application.Json;

        var result = new
        {
            StatusCode = Response.StatusCode,
            Message = _failureReason switch
            {
                AuthenticationFailureReason.API_KEY_HEADER_NOT_PROVIDED                              => "API-Key not provided",
                AuthenticationFailureReason.API_KEY_HEADER_VALUE_NULL                                => "API-Key value is null",
                AuthenticationFailureReason.NONE or AuthenticationFailureReason.API_KEY_INVALID or _ => "API-Key is not valid"
            }
        };

        using var responseStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(responseStream, result);
        await Response.BodyWriter.WriteAsync(responseStream.ToArray());
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.Headers.Append(HeaderNames.WWWAuthenticate, $@"Authorization realm=""{ApiKeyAuthenticationOptions.DefaultScheme}""");
        Response.StatusCode = StatusCodes.Status403Forbidden;
        Response.ContentType = MediaTypeNames.Application.Json;

        var result = new
        {
            StatusCode = Response.StatusCode,
            Message = "Forbidden"
        };

        using var responseStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(responseStream, result);
        await Response.BodyWriter.WriteAsync(responseStream.ToArray());
    }

    #region Privates

    private bool TryGetApiKeyHeader(out string apiKeyHeaderValue, out AuthenticateResult result)
    {
        apiKeyHeaderValue = null;
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            _logger.LogError("API-Key header not provided");

            _failureReason = AuthenticationFailureReason.API_KEY_HEADER_NOT_PROVIDED;
            result = AuthenticateResult.Fail("ApiKey header not provided");

            return false;
        }

        apiKeyHeaderValue = apiKeyHeaderValues.FirstOrDefault();
        if (apiKeyHeaderValues.Count == 0 || string.IsNullOrWhiteSpace(apiKeyHeaderValue))
        {
            _logger.LogError("API-Key header value null");

            _failureReason = AuthenticationFailureReason.API_KEY_HEADER_VALUE_NULL;
            result = AuthenticateResult.Fail("ApiKey header value null");

            return false;
        }

        result = null;
        return true;
    }

    private async Task<ApiKey> ApiKeyCheckAsync(string apiKey)
    {
        var ctx = await factory.CreateDbContextAsync();
        return await ctx.ApiKeys.FirstOrDefaultAsync(x => x.Key == apiKey);
    }

    #endregion
}

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";

    public static string Scheme => DefaultScheme;
    public static string AuthenticationType => DefaultScheme;
}

public static class AuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddApiKeySupport(this AuthenticationBuilder authenticationBuilder, Action<ApiKeyAuthenticationOptions> options)
        => authenticationBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, options);
}