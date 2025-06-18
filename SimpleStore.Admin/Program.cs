using System.Security.Claims;
using Ansari.Frontend.Services.Handlers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using SimpleStore.Admin;
using SimpleStore.Admin.Config;
using SimpleStore.Admin.Services.v1;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OpenIdConfig>(builder.Configuration.GetSection("OpenId"));
builder.Services.Configure<ApiConfig>(builder.Configuration.GetSection("API"));

builder.Services.Configure<ForwardedHeadersOptions>(options => { options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto; });
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

#if DEBUG
builder.Services.AddSassCompiler();
#endif

builder.Services.AddScoped<OpenIdConfig>();
builder.Services.AddScoped<ApiConfig>();

builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<AccessTokenHandler>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
        options.Cookie.Name = "SimpleStore";
        options.Cookie.Path = "/; SameSite=None";
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    .AddOpenIdConnect(options =>
    {
        var authority = builder.Configuration["OpenId:Authority"];
        var clientId = builder.Configuration["OpenId:ClientId"];
        var clientSecret = builder.Configuration["OpenId:ClientSecret"];

        if (authority == null) throw new MissingFieldException("OpenId authority missing");
        if (clientId == null) throw new MissingFieldException("OpenId client-id missing");
        if (clientSecret == null) throw new MissingFieldException("OpenId client-secret missing");
        
        options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.AccessDeniedPath = "/access-denied";
        options.SignedOutRedirectUri = "/signed-out";
        options.Authority = authority;
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.ClaimActions.MapUniqueJsonKey(ClaimsIdentity.DefaultRoleClaimType, "roles");
        options.UseTokenLifetime = false;
        options.RequireHttpsMetadata = builder.Environment.IsProduction() && !(builder.Configuration["DisableHttpsMetadata"] != null && builder.Configuration["DisableHttpsMetadata"] == "true");
        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("profile");
        options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "name", RoleClaimType = ClaimTypes.Role };

        options.Events = new OpenIdConnectEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                ctx.HandleResponse(); // Suppress the exception.
                ctx.Response.Redirect($"/error?error={Uri.EscapeDataString(ctx.Exception.Message[..Math.Min(1024, ctx.Exception.Message.Length)])}");

                return Task.CompletedTask;
            },

            OnRemoteFailure = ctx =>
            {
                ctx.Response.Redirect("/error");
                ctx.HandleResponse();

                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddHttpClient("api", client =>
{
    var endpoint = builder.Configuration["API:Endpoint"];
    var key = builder.Configuration["API:Key"];

    // If the application wants to use an API key, add it to the http client's header.
    if (key != null)
    {
        client.DefaultRequestHeaders.Add("X-API-Key", key);
    }

    client.BaseAddress = new Uri(endpoint ?? throw new MissingMemberException("missing endpoint"));
    client.Timeout = TimeSpan.FromMinutes(30);
}).AddHttpMessageHandler<AccessTokenHandler>();

builder.Services.AddScoped<SimpleStoreClient>(x =>
{
    var factory = x.GetRequiredService<IHttpClientFactory>();
    var endpoint = builder.Configuration["API:Endpoint"] ?? throw new MissingMemberException("missing endpoint");
    var httpClient = factory.CreateClient("api");

    return new SimpleStoreClient(endpoint, httpClient);
});

builder.Services.Configure<ForwardedHeadersOptions>(options => { options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto; });

var app = builder.Build();

app.Use((context, next) =>
{
    context.Request.Scheme = "https";
    return next(context);
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseForwardedHeaders();
    IdentityModelEventSource.ShowPII = true;
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseForwardedHeaders();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();