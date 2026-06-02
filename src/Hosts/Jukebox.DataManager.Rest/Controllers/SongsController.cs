using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Song;
using Jukebox.DataManager.Managers;
using Jukebox.DataManager.Managers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jukebox.DataManager.Rest.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SongsController(ISongManager songManager) : ControllerBase
{
    private readonly ISongManager _songManager = songManager;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSong(int id, CancellationToken cancellationToken)
    {
        var managerRequest = new ManagerRequest<int>
        {
            UserId = User.Identity?.Name ?? HttpContext.TraceIdentifier,
            Data = id
        };
        var response = await _songManager.FindByIdAsync(managerRequest, cancellationToken);
        return response.Success ? Ok(response.Data) : NotFound(response.ErrorMessage);
    }

    [HttpGet]
    public async Task<IActionResult> ListSongs([FromQuery] ListSongsRequest request, CancellationToken cancellationToken)
    {
        var managerRequest = new ManagerRequest<ListSongsRequest>
        {
            UserId = GetUserId(),
            Data = request
        };

        var result = await _songManager.ListAsync(managerRequest, cancellationToken);

        if (!result.Success)
            return StatusCode(500, result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> AddSong(AddSongRequest request, CancellationToken cancellationToken)
    {
        var managerRequest = new ManagerRequest<AddSongRequest>
        {
            UserId = User.Identity?.Name ?? HttpContext.TraceIdentifier,
            Data = request
        };
        var response = await _songManager.AddSongAsync(managerRequest, cancellationToken);
        return response.Success
            ? CreatedAtAction(nameof(GetSong), new { id = response.Data!.Id }, response.Data)
            : BadRequest(response.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSong(int id, [FromBody] UpdateSongRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest("Route id does not match request id");

        var managerRequest = new ManagerRequest<UpdateSongRequest>
        {
            UserId = GetUserId(),
            Data = request
        };

        var result = await _songManager.UpdateSongAsync(managerRequest, cancellationToken);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSong(int id, CancellationToken cancellationToken)
    {
        var managerRequest = new ManagerRequest<int>
        {
            UserId = User.Identity?.Name ?? HttpContext.TraceIdentifier,
            Data = id
        };
        var response = await _songManager.DeleteSongAsync(managerRequest, cancellationToken);
        return response.Success ? NoContent() : BadRequest(response.ErrorMessage);
    }

    private string GetUserId() =>
        User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}