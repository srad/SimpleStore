using Microsoft.EntityFrameworkCore;
using SimpleStore.Helpers.Interfaces;
using SimpleStore.Models;
using SimpleStore.Models.DTO;
using SimpleStore.Services.Interfaces;

namespace SimpleStore.Services;

/// <summary>
/// Could optionally also use a storage strategy.
/// </summary>
/// <param name="context"></param>
/// <param name="slug"></param>
/// <param name="accessor"></param>
public class BucketsService(IDbContextFactory<ApplicationDbContext> factory, ISlug slug, IHttpContextAccessor _httpContextAccessor) : IBucketsService
{
    private readonly string _storagePath = Environment.GetEnvironmentVariable("STORAGE_DIRECTORY") ?? throw new MissingFieldException("storage directory missing");
    private readonly string _url = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}";

    private string BucketPath(string directoryName) => Path.Combine(_storagePath, directoryName);

    public async Task<IReadOnlyList<BucketViewDto>> ToListAsync()
    {
        var context = await factory.CreateDbContextAsync();
        
        return await context.Buckets
            .AsNoTracking()
            .Select(x => new BucketViewDto
            {
                BucketId = x.BucketId,
                CreatedAt = x.CreatedAt,
                LastAccess = default,
                FileCount = x.Files.Count,
                Private = x.Private,
                Files = null,
                DirectoryName = x.DirectoryName,
                Name = x.Name,
                AsDownload = x.AsDownload
            })
            .ToListAsync();
    }

    public async Task<BucketViewDto> FindByNameAsync(string name)
    {
        var context = await factory.CreateDbContextAsync();
        
        return await context.Buckets
            .AsNoTracking()
            .Include(x => x.Files)
            .Select(bucket => new BucketViewDto
            {
                CreatedAt = bucket.CreatedAt,
                BucketId = bucket.BucketId,
                Name = bucket.Name,
                FileCount = bucket.Files.Count,
                DirectoryName = bucket.DirectoryName,

                Files = bucket.Files.Select(file => new FileViewDto
                {
                    FileName = file.FileName,
                    RelativeUrl = file.Url,
                    AbsoluteUrl = _url + file.Url,
                    CreatedAt = file.CreatedAt,
                    FileSizeMB = file.FileSizeMB,
                    FileSize = file.FileSize,
                    AccessCount = file.AccessCount,
                    LastAccess = default,
                    Private = false,
                    StorageFileId = file.StorageFileId,
                    AsDownload = file.AsDownload
                }).ToList(),
            })
            .FirstAsync(x => x.Name == name);
    }

    public async Task<BucketViewDto> FindById(string id)
    {
        var context = await factory.CreateDbContextAsync();
        
        return await context.Buckets
            .AsNoTracking()
            .Include(bucket => bucket.Files)
            .Select(bucket => new BucketViewDto
            {
                CreatedAt = bucket.CreatedAt,
                BucketId = bucket.BucketId,
                Name = bucket.Name,
                FileCount = bucket.Files.Count(),
                DirectoryName = bucket.DirectoryName,
                Files = bucket.Files.Select(file => new FileViewDto
                {
                    FileName = file.FileName,
                    RelativeUrl = file.Url,
                    CreatedAt = file.CreatedAt,
                    FileSizeMB = file.FileSizeMB,
                    LastAccess = file.LastAccess,
                    Private = file.Private,
                    StorageFileId = file.StorageFileId,
                    FileSize = file.FileSize,
                    AccessCount = file.AccessCount,
                    AbsoluteUrl = $"{_url}/{file.Url}",
                    AsDownload = file.AsDownload
                }).ToList(),
            })
            .FirstAsync(x => x.BucketId == id);
    }

    public async Task<BucketViewDto> CreateAsync(string name)
    {
        var context = await factory.CreateDbContextAsync();
        var slug1 = slug.Generate(name);

        if (await context.Buckets.AnyAsync(x => x.Name == name || x.DirectoryName == slug1))
        {
            throw new Exception($"The bucket name '{name}' is already in use");
        }

        var timestamp = DateTimeOffset.Now;
        var bucket = new Bucket
        {
            BucketId = Guid.NewGuid().ToString(),
            Name = name,
            DirectoryName = slug1,
            CreatedAt = timestamp,
            LastAccess = timestamp,
        };

        Directory.CreateDirectory(BucketPath(bucket.DirectoryName));

        await context.Buckets.AddAsync(bucket);
        await context.SaveChangesAsync();

        return new BucketViewDto
        {
            BucketId = bucket.BucketId,
            Name = bucket.Name,
            DirectoryName = bucket.DirectoryName,
            CreatedAt = bucket.CreatedAt,
            LastAccess = bucket.LastAccess,
            FileCount = bucket.Size,
            Private = bucket.Private,
            AsDownload = bucket.AsDownload
        };
    }

    public async Task AsDownloadAsync(string id, bool download)
    {
        var context = await factory.CreateDbContextAsync();

        await context.Buckets
            .Where(x => x.BucketId == id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.AsDownload, download));
    }

    public async Task DeleteAsync(string id)
    {
        var context = await factory.CreateDbContextAsync();
        
        var bucket = await context.Buckets
            .Include(x => x.Files)
            .FirstOrDefaultAsync(x => x.BucketId == id);

        if (bucket == null)
        {
            throw new Exception("Bucket not found");
        }

        Directory.Delete(BucketPath(bucket.DirectoryName), true);

        await context.Buckets
            .Where(x => x.BucketId == id)
            .ExecuteDeleteAsync();
    }

    public async Task<bool> ExistsAsync(string name)
    {
        var context = await factory.CreateDbContextAsync();
        
        return await context.Buckets.AnyAsync(x => x.Name == name);
    }
}