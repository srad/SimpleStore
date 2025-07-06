using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using SimpleStore.Auth;
using SimpleStore.Helpers;
using SimpleStore.Helpers.Interfaces;
using SimpleStore.Http;
using SimpleStore.Models;
using SimpleStore.Seeds;
using SimpleStore.Services;
using SimpleStore.Services.Interfaces;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

// Handle data folders and paths.

var appDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
var defaultAppDirectory = Path.Combine(appDirectory, "SimpleStore");

if (Environment.GetEnvironmentVariable("STORAGE_DIRECTORY") == null)
{
    var defaultStoragePath = Path.Combine(defaultAppDirectory, "storage");
    Environment.SetEnvironmentVariable("STORAGE_DIRECTORY", defaultStoragePath);
}

if (Environment.GetEnvironmentVariable("DB_PATH") == null)
{
    var defaultDbPath = Path.Combine(defaultAppDirectory, "data.db");
    Environment.SetEnvironmentVariable("DB_PATH", defaultDbPath);
    Console.WriteLine("Using default DB path: " + defaultDbPath);
}

var storageDirectory = Environment.GetEnvironmentVariable("STORAGE_DIRECTORY");
if (storageDirectory == null)
{
    throw new ArgumentNullException("STORAGE_DIRECTORY missing");
}

if (!Directory.Exists(storageDirectory))
{
    Directory.CreateDirectory(storageDirectory);
}

Console.WriteLine($"DB directory: {Environment.GetEnvironmentVariable("DB_PATH")}");
Console.WriteLine($"Storage directory: {storageDirectory}");

// Build application.

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthentication()
    .AddApiKeySupport(options => { })
    .AddKeycloakJwtBearer(
        serviceName: "keycloak",
        realm: builder.Configuration["Realm"] ?? throw new InvalidOperationException("Realm not configured"),
        options =>
        {
            options.RequireHttpsMetadata = builder.Environment.IsProduction();
            options.Audience = "account";
            options.IncludeErrorDetails = !builder.Environment.IsProduction();
        });

builder.Services.AddAuthorization(options =>
{
    var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme, ApiKeyAuthenticationOptions.DefaultScheme);
    defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder
        .RequireAuthenticatedUser()
        .RequireRole("simplestore");
    options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
});

builder.Services.AddDbContextFactory<ApplicationDbContext>(options => { options.UseSqlite($"Data Source={Environment.GetEnvironmentVariable("DB_PATH")}"); });
builder.Services.AddScoped<IApiKeysService, ApiKeysService>();
//builder.Services.AddScoped<IAuthorizationFilter, ApiAuthorizationFilter>();
//builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();
builder.Services.AddScoped<StorageNameValidator>();
builder.Services.AddScoped<ISlug, StorageSlug>();
builder.Services.AddScoped<IAllowedHostsService, AllowedHostsService>();
builder.Services.AddScoped<IBucketsService, BucketsService>();
builder.Services.AddScoped<IStorageService<string>, StorageService>();
builder.Services.AddScoped<IKeyService, KeyService>();

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.MapType<decimal>(() => new OpenApiSchema { Type = "number", Format = "decimal" });
    o.MapType<decimal?>(() => new OpenApiSchema { Type = "number", Format = "decimal", Nullable = true });
    o.CustomOperationIds(e => $"{e.ActionDescriptor.RouteValues["controller"]}{e.ActionDescriptor.RouteValues["action"]}");
    o.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "SimpleStore API" });
});

builder.Services.AddOutputCache(options => { options.AddBasePolicy(b => b.Cache()); });

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        // Optional: create a scope in case you need scoped services later
        using var scope = app.Services.CreateScope();

        var swaggerProvider = scope.ServiceProvider
            .GetRequiredService<ISwaggerProvider>();

        // The version string must match the one you registered with SwaggerDoc(...)
        var openApiDocument = swaggerProvider.GetSwagger("v1");

        // Convert the document to JSON (you can use OpenApiSpecVersion.OpenApi2_0
        // and/or OpenApiFormat.Yaml if you prefer)
        var json = openApiDocument.Serialize(
            OpenApiSpecVersion.OpenApi3_0,
            OpenApiFormat.Json);

        var outputPath = Path.Combine(
            "Docs",
            "swagger.json");

        // Ensure the directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        File.WriteAllText(outputPath, json);

        Console.WriteLine($"OpenAPI specification written to: {outputPath}");
    });

    app.UseSwagger();
    app.UseSwaggerUI();
    IdentityModelEventSource.ShowPII = true;
}

app.UseDbInit();

//app.UseHttpsRedirection();
app.UseAuthorization();
//app.UseAuthentication();
app.MapControllers();

app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
    FileProvider = new PhysicalFileProvider(storageDirectory),
    RequestPath = "",
    OnPrepareResponse = ctx => StaticFileOptionsHandler.OnPrepareResponse(app, ctx)
});

app.Run();