using Microsoft.EntityFrameworkCore;
using SimpleStore.Models;
using SimpleStore.Services.Interfaces;

namespace SimpleStore.Services;

public class AllowedHostsService(IDbContextFactory<ApplicationDbContext> factory) : IAllowedHostsService
{
    public async Task DeleteAsync(string host)
    {
        var context = await factory.CreateDbContextAsync();
        
        if (await context.AllowedHosts.CountAsync() == 1)
        {
            throw new Exception("At least one host must remain in database");
        }

        await context.AllowedHosts.Where(x => x.Hostname == host).ExecuteDeleteAsync();
    }
    
    public async Task<IReadOnlyList<AllowedHost>> ToListAsync()
    {
        var context = await factory.CreateDbContextAsync();
        
        return await context.AllowedHosts.ToListAsync();
    }

    public async Task<AllowedHost> CreateAsync(string host)
    {
        // Validation
        host = host.Trim().ToLower();

        if (host.Contains("http"))
        {
            throw new Exception("Please provide only the hostname in the format 'host' without any prefixes or ports");
        }

        if (Uri.CheckHostName(host) == UriHostNameType.Unknown)
        {
            throw new Exception($"The hostname '{host}' is invalid");
        }
        
        var context = await factory.CreateDbContextAsync();

        if (await context.AllowedHosts.AnyAsync(x => x.Hostname == host))
        {
            throw new Exception($"Hostname '{host}' already exists");
        }

        var newHost = new AllowedHost { Hostname = host };
        await context.AllowedHosts.AddAsync(newHost);
        await context.SaveChangesAsync();

        return newHost;
    }
}