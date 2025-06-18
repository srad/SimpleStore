using SimpleStore.Models;

namespace SimpleStore.Services.Interfaces;

public interface IAllowedHostsService
{
    Task DeleteAsync(string host);
    Task<IReadOnlyList<AllowedHost>> ToListAsync();
    Task<AllowedHost> CreateAsync(string host);
}