using SimpleStore.Models;
using SimpleStore.Models.DTO;

namespace SimpleStore.Services.Interfaces;

public interface IBucketsService
{
    Task<IReadOnlyList<BucketViewDto>> ToListAsync();
    Task<BucketViewDto> FindByNameAsync(string name);
    Task<BucketViewDto> FindById(string id);
    Task<BucketViewDto> CreateAsync(string name);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string name);
    Task AsDownloadAsync(string id, bool download);
}