using Jukebox.DataManager.Contracts.DataContracts.Artist;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Managers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ArtistsController(IArtistManager artistManager, ILogger<ArtistsController> logger) : ControllerBase
{
    private readonly IArtistManager _artistManager = artistManager;
    private readonly ILogger<ArtistsController> _logger = logger;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetArtist(int id, CancellationToken cancellationToken)
    {
        var request = new ManagerRequest<int>
        {
            UserId = GetUserId(),
            Data = id
        };

        var result = await _artistManager.FindByIdAsync(request, cancellationToken);

        if (!result.Success)
            return NotFound(result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> ListArtists([FromQuery] ListArtistsRequest request, CancellationToken cancellationToken)
    {
        var managerRequest = new ManagerRequest<ListArtistsRequest>
        {
            UserId = GetUserId(),
            Data = request
        };

        var result = await _artistManager.ListAsync(managerRequest, cancellationToken);

        if (!result.Success)
            return StatusCode(500, result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> AddArtist([FromBody] AddArtistRequest request, CancellationToken cancellationToken)
    {
        var managerRequest = new ManagerRequest<AddArtistRequest>
        {
            UserId = GetUserId(),
            Data = request
        };

        var result = await _artistManager.AddArtistAsync(managerRequest, cancellationToken);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return result.Success
            ? CreatedAtAction(nameof(GetArtist), new { id = result.Data!.Id }, result.Data)
            : BadRequest(result.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateArtist(int id, [FromBody] UpdateArtistRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest("Route id does not match request id");

        var managerRequest = new ManagerRequest<UpdateArtistRequest>
        {
            UserId = GetUserId(),
            Data = request
        };

        var result = await _artistManager.UpdateArtistAsync(managerRequest, cancellationToken);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArtist(int id, CancellationToken cancellationToken)
    {
        var request = new ManagerRequest<int>
        {
            UserId = GetUserId(),
            Data = id
        };

        var result = await _artistManager.DeleteArtistAsync(request, cancellationToken);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return NoContent();
    }

    private string GetUserId() =>
    User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}