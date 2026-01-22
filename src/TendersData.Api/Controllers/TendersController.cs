using MediatR;
using Microsoft.AspNetCore.Mvc;
using TendersData.Application.Tenders.Queries.GetFilteredTenders;
using TendersData.Application.Tenders.Queries.GetTenderById;

namespace TendersData.Api.Controllers;

/// <summary>
/// Controller for managing tenders data
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TendersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Gets all tenders with optional filtering and pagination
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination (PageNumber, PageSize, MinPrice, MaxPrice, StartDate, EndDate, SortBy, SortOrder)</param>
    /// <returns>Paged list of tenders</returns>
    /// <response code="200">Returns the list of tenders</response>
    /// <response code="400">If validation fails</response>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetFilteredTendersQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific tender by its ID
    /// </summary>
    /// <param name="id">The ID of the tender (must be greater than 0)</param>
    /// <returns>The tender with the specified ID</returns>
    /// <response code="200">Returns the requested tender</response>
    /// <response code="400">If id is less than or equal to 0 (validation error)</response>
    /// <response code="404">If the tender with the specified ID is not found</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetTenderByIdQuery(id);
        var result = await mediator.Send(query);

        return Ok(result);
    }
}
