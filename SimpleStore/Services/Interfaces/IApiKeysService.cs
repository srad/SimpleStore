using SimpleStore.Models;

namespace SimpleStore.Services.Interfaces;

public interface IApiKeysService
{
    Task<IReadOnlyList<ApiKey>> ToListAsync();
    Task<ApiKey> CreateAsync(string title);
    Task DeleteAsync(string key);
}