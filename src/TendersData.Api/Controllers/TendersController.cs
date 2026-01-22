using MediatR;
using Microsoft.AspNetCore.Mvc;
using TendersData.Application.Tenders.Queries.GetFilteredTenders;
using TendersData.Application.Tenders.Queries.GetTenderById;

namespace TendersData.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TendersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetFilteredTendersQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetTenderByIdQuery(id);
        var result = await mediator.Send(query);

        return Ok(result);
    }
}
