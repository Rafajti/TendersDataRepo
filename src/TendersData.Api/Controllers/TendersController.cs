using Microsoft.AspNetCore.Mvc;

namespace TendersData.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TendersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        return Ok(true);
    }
}
