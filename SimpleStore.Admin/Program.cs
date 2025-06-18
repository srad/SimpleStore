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

builder.AddServiceDefaults();

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
    .AddKeycloakOpenIdConnect(
        serviceName: "keycloak",
        realm: builder.Configuration["Authentication:OpenIdConnect:Realm"] ?? throw new InvalidOperationException("No Realm configured"),
        options =>
        {
            options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
            options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.AccessDeniedPath = "/access-denied";
            options.SignedOutRedirectUri = "/signed-out";
            options.ClientId = builder.Configuration["Authentication:OpenIdConnect:ClientId"];
            options.ClientSecret = builder.Configuration["Authentication:OpenIdConnect:ClientSecret"];
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
            options.Scope.Add("roles");
            options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = "name", RoleClaimType = ClaimTypes.Role };

            options.Events = new OpenIdConnectEvents
            {
                OnAuthenticationFailed = ctx =>
                {
                    ctx.HandleResponse(); // Suppress the exception.
                    ctx.Response.Redirect($"/error?error={Uri.EscapeDataString(ctx.Exception.Message[..Math.Min(1024, ctx.Exception.Message.Length)])}");

                    return Task.CompletedTask;
                },
                
                OnTokenValidated = ctx =>
                {
                    var logger = ctx.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();

                    foreach (var c in ctx.Principal!.Claims)
                        logger.LogInformation("CLAIM {Type} = {Value}", c.Type, c.Value);

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
    var key = builder.Configuration["apikey"];

    // If the application wants to use an API key, add it to the http client's header.
    if (key != null)
    {
        client.DefaultRequestHeaders.Add("X-API-Key", key);
    }

    client.BaseAddress = new Uri("https+http://api");
    client.Timeout = TimeSpan.FromMinutes(30);
}).AddHttpMessageHandler<AccessTokenHandler>();

builder.Services.AddScoped<SimpleStoreClient>(x =>
{
    var factory = x.GetRequiredService<IHttpClientFactory>();
    var httpClient = factory.CreateClient("api");

    return new SimpleStoreClient("https+http://api", httpClient);
});

builder.Services.Configure<ForwardedHeadersOptions>(options => { options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto; });

var app = builder.Build();

app.MapDefaultEndpoints();

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