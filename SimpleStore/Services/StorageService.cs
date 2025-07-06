using Microsoft.EntityFrameworkCore;
using SimpleStore.Helpers.Interfaces;
using SimpleStore.Models;
using SimpleStore.Models.DTO;
using SimpleStore.Services.Interfaces;

namespace SimpleStore.Services;

public class StorageService(IDbContextFactory<ApplicationDbContext> factory, ISlug slug, ILogger<StorageService> logger, IHttpContextAccessor httpContextAccessor) : IStorageService<string>
{
    private readonly string _storagePath = Environment.GetEnvironmentVariable("STORAGE_DIRECTORY") ?? throw new ArgumentNullException();
    private string BucketPath(string directoryName) => Path.Combine(_storagePath, directoryName);

    private readonly string _url = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}{httpContextAccessor.HttpContext.Request.PathBase}";

    public async Task<IReadOnlyList<BucketFile>> ToListAsync()
    {
        await using var context = await factory.CreateDbContextAsync();

        return await context.BucketFiles.AsNoTracking().ToListAsync();
    }

    public async Task<BucketFile> FindByIdAsync(string id)
    {
        await using var context = await factory.CreateDbContextAsync();

        var storageFile = await context.BucketFiles.FindAsync(id);

        if (storageFile == null)
        {
            throw new Exception("Storage item not found");
        }

        storageFile.Url = _url + storageFile;

        return storageFile;
    }

    /// <summary>
    /// Notice that this function slugs the filename before comparing it.
    /// It does no raw comparison of names.
    /// </summary>
    /// <param name="bucketId"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public async Task<bool> ExistsAsync(string bucketId, string fileName)
    {
        await using var context = await factory.CreateDbContextAsync();

        return await context.BucketFiles.AnyAsync(x => x.BucketId == bucketId && x.StoredFileName == slug.Generate(fileName));
    }

    public async Task<IReadOnlyList<CreateFileDto>> SaveAsync(string bucketId, List<IFormFile> files)
    {
        await using var context = await factory.CreateDbContextAsync();

        var bucket = await context.Buckets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BucketId == bucketId);

        if (bucket == null)
        {
            throw new Exception("Bucket not found");
        }

        var results = new List<CreateFileDto>();
        foreach (var file in files)
        {
            try
            {
                // Either a random filename is produced or the original converted into a slug.

                //var ext = Path.GetExtension(file.FileName);
                //fileNameSlug = $"{Guid.NewGuid().ToString()}{ext}";
                // Create slug and check if already exists.
                var fileNameSlug = slug.Generate(file.FileName);
                var fileExists = await context.BucketFiles.AnyAsync(x => x.BucketId == bucketId && x.StoredFileName == fileNameSlug);
                if (fileExists)
                {
                    var bucketFile = await context.BucketFiles.FirstAsync(x => x.BucketId == bucketId && x.StoredFileName == fileNameSlug);

                    results.Add(new CreateFileDto
                    {
                        BucketFile = bucketFile,
                        Success = false,
                        ErrorMessage = $"A file '{fileNameSlug}' already exists in this bucket"
                    });
                    continue;
                }

                // Upload
                var filePath = Path.Combine(BucketPath(bucket.DirectoryName), fileNameSlug);
                await using var stream = File.OpenWrite(filePath);
                await file.CopyToAsync(stream);

                var storedFile = new BucketFile
                {
                    StorageFileId = Guid.NewGuid().ToString(),
                    FileName = file.FileName,
                    StoredFileName = fileNameSlug,
                    FilePath = filePath,
                    CreatedAt = DateTimeOffset.Now,
                    FileSize = stream.Length,
                    FileSizeMB = String.Format("{0:0.00}", (float)stream.Length / 1024 / 1024),
                    Private = false,
                    AccessCount = 0,
                    Url = $"{bucket.DirectoryName}/{fileNameSlug}",
                    BucketId = bucketId,
                    LastAccess = DateTimeOffset.Now,
                };
                await context.BucketFiles.AddAsync(storedFile);
                await context.SaveChangesAsync();

                results.Add(new CreateFileDto
                {
                    BucketFile = storedFile,
                    ErrorMessage = null,
                    Success = true,
                });
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                results.Add(new CreateFileDto()
                {
                    ErrorMessage = $"Error saving {file.FileName}: {e.Message}",
                    Success = false
                });
            }
        }

        return results;
    }

    public async Task DeleteAsync(string id)
    {
        await using var context = await factory.CreateDbContextAsync();

        var storageFile = await context.BucketFiles.FindAsync(id);
        if (storageFile == null)
        {
            throw new Exception("File not found");
        }

        try
        {
            logger.LogInformation("Deleting file '{FilePath}'", storageFile.FilePath);
            File.Delete(storageFile.FilePath);
            context.BucketFiles.Remove(storageFile);
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            throw new Exception($"Error deleting '{storageFile.FilePath}': {e.Message}");
        }
    }

    public async Task MakePrivateAsync(string id)
    {
        await using var context = await factory.CreateDbContextAsync();

        await context.BucketFiles
            .Where(x => x.StorageFileId == id)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.Private, true));
    }

    public async Task MakePublicAsync(string id)
    {
        await using var context = await factory.CreateDbContextAsync();

        await context.BucketFiles
            .Where(x => x.StorageFileId == id)
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.Private, false));
    }

    public StorageInfoDto GetStorageStatsAsync()
    {
        var drive = new DriveInfo(_storagePath);
        var free = (float)drive.TotalFreeSpace / 1024 / 1024 / 1024;
        var total = (float)drive.TotalSize / 1024 / 1024 / 1024;

        return new StorageInfoDto
        {
            FreeGB = free,
            SizeGB = total,
            AvailablePercent = free / total * 100,
            Name = drive.Name
        };
    }

    public async Task AsDownloadAsync(string id, bool download)
    {
        await using var context = await factory.CreateDbContextAsync();

        await context.BucketFiles
            .Where(x => x.StorageFileId == id)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.AsDownload, download));
    }
}