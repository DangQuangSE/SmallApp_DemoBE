using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SecondBike.Application.Common;

namespace SecondBike.Api.Controllers;

/// <summary>
/// Base API controller with common helpers for extracting the current user
/// and mapping <see cref="Result{T}"/> / <see cref="Result"/> to HTTP responses.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Extracts the current authenticated user's AppUser Id from the JWT claims.
    /// </summary>
    protected int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null ? int.Parse(claim.Value) : 0;
    }

    /// <summary>
    /// Maps a typed result to the appropriate HTTP response.
    /// </summary>
    protected IActionResult ToResponse<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Data);

        return BadRequest(new { error = result.ErrorMessage, errors = result.Errors });
    }

    /// <summary>
    /// Maps a void result to the appropriate HTTP response.
    /// </summary>
    protected IActionResult ToResponse(Result result)
    {
        if (result.IsSuccess)
            return Ok(new { message = "Success" });

        return BadRequest(new { error = result.ErrorMessage });
    }
}
