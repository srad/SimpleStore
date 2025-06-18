using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleStore.Models;
using SimpleStore.Services.Interfaces;

namespace SimpleStore.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
[Authorize]
public class AllowedHostsController(IAllowedHostsService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<AllowedHost>> GetAsync() => service.ToListAsync();

    [HttpDelete($"{{{nameof(host)}}}")]
    public Task DeleteAsync(string host) => service.DeleteAsync(host);

    /// <summary>
    /// Adds a new white listed host name.
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    [HttpPost($"{{{nameof(host)}}}")]
    public Task<AllowedHost> CreateAsync(string host) =>  service.CreateAsync(host);
}