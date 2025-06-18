using Microsoft.EntityFrameworkCore;
using SimpleStore.Models;
using SimpleStore.Services.Interfaces;

namespace SimpleStore.Services;

public class ApiKeysService(IDbContextFactory<ApplicationDbContext> factory, IKeyService keyService) : IApiKeysService
{
    public async Task<IReadOnlyList<ApiKey>> ToListAsync()
    {
        var context = await factory.CreateDbContextAsync();
        
        return await context.ApiKeys.ToListAsync();
    }

    public async Task<ApiKey> CreateAsync(string title)
    {
        var key = new ApiKey
        {
            Key = keyService.GenerateKey(), Title = title, CreatedAt = DateTimeOffset.UtcNow, AccessTimeLimited = false
        };
        
        var context = await factory.CreateDbContextAsync();
        await context.ApiKeys.AddAsync(key);
        await context.SaveChangesAsync();

        return key;
    }
    
    public async Task DeleteAsync(string key)
    {
        var context = await factory.CreateDbContextAsync();
        
        if (await context.ApiKeys.CountAsync() == 1)
        {
            throw new Exception("At least one key must remain in database");
        }
        
        await context.ApiKeys.Where(x => x.Key == key).ExecuteDeleteAsync();
    }
}