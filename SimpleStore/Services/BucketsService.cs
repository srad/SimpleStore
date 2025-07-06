using Microsoft.EntityFrameworkCore;
using SimpleStore.Helpers.Interfaces;
using SimpleStore.Models;
using SimpleStore.Models.DTO;
using SimpleStore.Services.Interfaces;

namespace SimpleStore.Services;

public class BucketsService(IDbContextFactory<ApplicationDbContext> factory, ISlug slug, IHttpContextAccessor httpContextAccessor, ILogger<BucketsService> logger) : IBucketsService
{
    private readonly string _storagePath = Environment.GetEnvironmentVariable("STORAGE_DIRECTORY") ?? throw new MissingFieldException("storage directory missing");
    private readonly string _url = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}";

    private string BucketPath(string directoryName) => Path.Combine(_storagePath, directoryName);

    public async Task<IReadOnlyList<BucketViewDto>> ToListAsync()
    {
        await using var context = await factory.CreateDbContextAsync();
        
        return await context.Buckets
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
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
        await using var context = await factory.CreateDbContextAsync();
        
        return await context.Buckets
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
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
        await using var context = await factory.CreateDbContextAsync();
        
        return await context.Buckets
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Include(bucket => bucket.Files)
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
        await using var context = await factory.CreateDbContextAsync();
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
        await using var context = await factory.CreateDbContextAsync();

        await context.Buckets
            .Where(x => x.BucketId == id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.AsDownload, download));
    }
    
    public async Task SoftDeleteAsync(string id)
    {
        await using var context = await factory.CreateDbContextAsync();

        var bucket = await context.Buckets.FirstOrDefaultAsync(b => b.BucketId == id);
        if (bucket == null) throw new Exception("Bucket not found");

        bucket.IsDeleted = true;

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        logger.LogInformation("Try deleting bucket {Id} ...", id);
        
        await using var context = await factory.CreateDbContextAsync();
        
        var bucket = await context.Buckets
            .Include(x => x.Files)
            .FirstOrDefaultAsync(x => x.BucketId == id);

        if (bucket == null)
        {
            throw new KeyNotFoundException("Bucket not found");
        }

        var directoryPath = BucketPath(bucket.DirectoryName);

        // Delete folder from disk and database in one transaction.
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Whatever happens, first mark as deleted in a separate transaction.
            // If the actual deletion fails, it can be retried by whatever async process.
            await SoftDeleteAsync(id);
            
            // First: delete the bucket and files from DB
            context.Buckets.Remove(bucket);
            await context.SaveChangesAsync();

            // Then: delete the directory from disk
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive: true);
            }

            // Commit DB transaction
            await transaction.CommitAsync();
            logger.LogInformation("Bucket {Id} deleted", id);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            logger.LogError("Error deleting bucket: {Message}", e.Message);

            // Optional: log and/or try to restore the DB/file state if needed
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string name)
    {
        await using var context = await factory.CreateDbContextAsync();
        
        return await context.Buckets.AnyAsync(x => x.Name == name);
    }
}