using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleStore.Models;
using SimpleStore.Models.DTO;
using SimpleStore.Services.Interfaces;

namespace SimpleStore.Controllers;

[Route("api/v1/[controller]"), ApiController, Produces("application/json")]
[Authorize]
public class StorageController(IStorageService<string> service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<BucketFile>> GetFilesAsync() => service.ToListAsync();

    [HttpGet($"{{{nameof(id)}}}")]
    public Task<BucketFile> GetFileAsync(string id) => service.FindByIdAsync(id);

    [HttpGet($"exists/{{{nameof(bucketId)}}}/{{{nameof(fileName)}}}")]
    public Task<bool> ExistsAsync(string bucketId, string fileName) => service.ExistsAsync(bucketId, fileName);

    [HttpPost("{bucketId}")]
    public Task<IReadOnlyList<CreateFileDto>> SaveFileAsync(string bucketId, [FromForm] List<IFormFile> files) => service.SaveAsync(bucketId, files);

    [HttpDelete($"{{{nameof(id)}}}")]
    public Task DeleteAsync(string id) => service.DeleteAsync(id);

    [HttpPost("private")]
    public Task PrivateAsync(string id) => service.PrivateAsync(id);

    [HttpPost("public")]
    public Task PublicAsync(string id) => service.PublicAsync(id);

    [HttpGet("storage_info")]
    public StorageInfoDto GetInfoAsync() => service.GetStorageStatsAsync();
    
    [HttpPatch]
    public Task AsDownloadAsync(string id, bool asDownload) => service.AsDownloadAsync(id, asDownload);
}