using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Genre;
using Jukebox.DataManager.Managers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jukebox.DataManager.Rest.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class GenresController(IGenreManager genreManager) : ControllerBase
{
    private readonly IGenreManager _genreManager = genreManager;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGenre(int id, CancellationToken cancellationToken)
    {
        var managerRequest = new ManagerRequest<int>
        {
            UserId = GetUserId(),
            Data = id
        };
        var response = await _genreManager.FindByIdAsync(managerRequest, cancellationToken);
        return response.Success ? Ok(response.Data) : NotFound(response.ErrorMessage);
    }

    [HttpGet]
    public async Task<IActionResult> ListGenres([FromQuery] ListGenresRequest request, CancellationToken cancellationToken)
    {
        var managerRequest = new ManagerRequest<ListGenresRequest>
        {
            UserId = GetUserId(),
            Data = request
        };

        var result = await _genreManager.ListAsync(managerRequest, cancellationToken);

        if (!result.Success)
            return StatusCode(500, result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> AddGenre(AddGenreRequest request, CancellationToken cancellationToken)
    {
        var managerRequest = new ManagerRequest<AddGenreRequest>
        {
            UserId = GetUserId(),
            Data = request
        };
        var response = await _genreManager.AddGenreAsync(managerRequest, cancellationToken);
        return response.Success
            ? CreatedAtAction(nameof(GetGenre), new { id = response.Data!.Id }, response.Data)
            : BadRequest(response.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGenre(int id, [FromBody] UpdateGenreRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest("Route id does not match request id");

        var managerRequest = new ManagerRequest<UpdateGenreRequest>
        {
            UserId = GetUserId(),
            Data = request
        };

        var result = await _genreManager.UpdateGenreAsync(managerRequest, cancellationToken);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGenre(int id, CancellationToken cancellationToken)
    {
        var managerRequest = new ManagerRequest<int>
        {
            UserId = GetUserId(),
            Data = id
        };
        var response = await _genreManager.DeleteGenreAsync(managerRequest, cancellationToken);
        return response.Success ? NoContent() : BadRequest(response.ErrorMessage);
    }

    private string GetUserId() =>
        User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}