using Microsoft.EntityFrameworkCore;
using SimpleStore.Models;

namespace SimpleStore.Controllers.Filters;

public class ApiKeyValidator(ApplicationDbContext context) : IApiKeyValidator
{
    public async Task<bool> IsValid(string apiKey)
    {
        return await context.ApiKeys.AnyAsync(x => x.Key == apiKey);
    }
}

public interface IApiKeyValidator
{
    public Task<bool> IsValid(string apiKey);
}