using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/controller")]
public class MyController : ControllerBase
{
    [HttpGet("resource")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> LimitedResource()
    {
        await Task.Delay(1000);

        var random = new Random();

        return Ok(random.Next(1, 101));
    }

    [HttpGet("resource2")]
    [DisableRateLimiting]
    public async Task<IActionResult> NotLimitedResource()
    {
        await Task.Delay(1000);

        var random = new Random();

        return Ok(random.Next(1, 101));
    }
}
