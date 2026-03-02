using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.DTOs.Bikes;
using SecondBike.Application.Interfaces.Services;

namespace SecondBike.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BikesController : ControllerBase
{
    private readonly IBikeSearchService _bikeSearchService;
    private readonly IBikePostService _bikePostService;

    public BikesController(IBikeSearchService bikeSearchService, IBikePostService bikePostService)
    {
        _bikeSearchService = bikeSearchService;
        _bikePostService = bikePostService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBikes([FromQuery] BikeFilterDto filter, CancellationToken ct)
    {
        var result = await _bikeSearchService.SearchAsync(filter, ct);
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBike(Guid id, CancellationToken ct)
    {
        var result = await _bikeSearchService.GetDetailAsync(id, ct);
        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        return NotFound(result.ErrorMessage);
    }
}
