using Microsoft.AspNetCore.Mvc;
using SimpleStore.Models;
using SimpleStore.Models.DTO;

namespace SimpleStore.Services.Interfaces;

public interface IStorageService<in T>
{
    Task<IReadOnlyList<BucketFile>> ToListAsync();
    Task<BucketFile> FindByIdAsync(T id);
    Task<bool> ExistsAsync(string bucketId, string fileName);
    Task<IReadOnlyList<CreateFileDto>> SaveAsync(string bucketId, List<IFormFile> files);
    Task DeleteAsync(T id);
    Task PrivateAsync(T id);
    Task PublicAsync(T id);
    StorageInfoDto GetStorageStatsAsync();
    Task AsDownloadAsync(string id, bool download);
}