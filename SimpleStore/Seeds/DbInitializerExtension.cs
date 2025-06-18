using Microsoft.EntityFrameworkCore;
using SimpleStore.Helpers.Interfaces;
using SimpleStore.Models;
using SimpleStore.Services.Interfaces;

namespace SimpleStore.Seeds;

internal static class DbInitializerExtension
{
    /// <summary>
    /// Also creates a default API key on launch, when none is available.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseDbInit(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app, nameof(app));

        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        if (context != null)
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }
            var services = scope.ServiceProvider;
            var service = services.GetRequiredService<IKeyService>();
            var slug = services.GetRequiredService<ISlug>();
            DbInitializer.Initialize(context, service, slug);
        }

        return app;
    }
}