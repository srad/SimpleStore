using System.Net;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using SimpleStore.Models;

namespace SimpleStore.Http;

/// <summary>
/// File responses handled here. One could also implement this as controller action. (MapControllers)
/// </summary>
public static class StaticFileOptionsHandler
{
    public static async void OnPrepareResponse(WebApplication app, StaticFileResponseContext ctx)
    {
        using var scope = app.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        await using var dbContext = await factory.CreateDbContextAsync();
        
        if (!await dbContext.AllowedHosts.AnyAsync(x => x.Hostname == "*" || x.Hostname == ctx.Context.Request.Host.ToString()))
        {
            ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            ctx.Context.Response.ContentLength = 0;
            ctx.Context.Response.Body = Stream.Null;
            ctx.Context.Response.Headers.Add("Cache-Control", "no-store");
            return;
        }
        
        var timestamp = DateTimeOffset.Now;

        var dir = new DirectoryInfo(ctx.File.PhysicalPath!);
        
        var file = await dbContext.BucketFiles
            .Include(x => x.Bucket)
            .FirstAsync(x => x.FilePath == dir.ToString());

        if ((file.Bucket.Private || file.Private) && !ctx.Context.User.Identity.IsAuthenticated)
        {
            ctx.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            ctx.Context.Response.ContentLength = 0;
            ctx.Context.Response.Body = Stream.Null;
            ctx.Context.Response.Headers.Add("Cache-Control", "no-store");
        }

        // Update timestamp for bucket
        await dbContext.Buckets.Where(x => x.BucketId == file.BucketId)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.LastAccess, timestamp));
        
        file.LastAccess = timestamp;
        file.AccessCount += 1;
        await dbContext.SaveChangesAsync();

        // Could add an option to the database that the file is "download only".
        if (file.Bucket.AsDownload || file.AsDownload)
        {
            ctx.Context.Response.Headers.ContentDisposition += "attachment;";
        }
        ctx.Context.Response.Headers.ContentDisposition += $"filename={file.FileName}";
    }
}