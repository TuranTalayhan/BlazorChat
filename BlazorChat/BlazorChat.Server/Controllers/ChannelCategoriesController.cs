using System.Security.Claims;
using BlazorChat.Server.Application.Features.ChannelCategories;
using BlazorChat.Server.Application.Features.ChannelCategories.Commands;
using BlazorChat.Shared.DTO;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorChat.Server.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class ChannelCategoriesController(IMediator mediator) : ControllerBase
{
    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 0;
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var result = await mediator.Send(new UpdateCategoryCommand(userId, id, dto), ct);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                CategoryError.Forbidden => Forbid(),
                CategoryError.NotFound => NotFound(),
                CategoryError.BadRequest => BadRequest(new { message = result.ErrorMessage }),
                _ => StatusCode(500)
            };
        }

        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Unauthorized();

        var result = await mediator.Send(new DeleteCategoryCommand(userId, id), ct);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                CategoryError.Forbidden => Forbid(),
                CategoryError.NotFound => NotFound(),
                _ => BadRequest()
            };
        }

        return NoContent();
    }
    
}