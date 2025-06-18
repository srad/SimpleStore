using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleStore.Models;
using SimpleStore.Services.Interfaces;

namespace SimpleStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize]
public class ApiKeysController(IApiKeysService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ApiKey>> GetAsync() => service.ToListAsync();

    [HttpDelete]
    public Task DeleteAsync(string key) => service.DeleteAsync(key);

    [HttpPost]
    public Task<ApiKey> CreateAsync(string title) => service.CreateAsync(title);
}