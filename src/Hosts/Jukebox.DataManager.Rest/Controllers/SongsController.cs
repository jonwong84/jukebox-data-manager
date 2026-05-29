using Jukebox.DataManager.Contracts;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Song;
using Jukebox.DataManager.Managers;
using Microsoft.AspNetCore.Mvc;

namespace Jukebox.DataManager.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SongsController(ISongManager songManager) : ControllerBase
{
    private readonly ISongManager _songManager = songManager;

    [HttpGet("{id}")]
    public async Task<ActionResult<SongDetails>> GetSong(int id, CancellationToken cancellationToken)
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
            UserId = HttpContext.TraceIdentifier,
            Data = request
        };

        var result = await _songManager.ListAsync(managerRequest, cancellationToken);

        if (!result.Success)
            return StatusCode(500, result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<SongSummary>> AddSong(AddSongRequest request, CancellationToken cancellationToken)
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
    public async Task<ActionResult<SongDetails>> UpdateSong(int id, UpdateSongRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        var managerRequest = new ManagerRequest<UpdateSongRequest>
        {
            UserId = User.Identity?.Name ?? HttpContext.TraceIdentifier,
            Data = request
        };
        var response = await _songManager.UpdateSongAsync(managerRequest, cancellationToken);
        return response.Success ? Ok(response.Data) : BadRequest(response.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSong(int id, CancellationToken cancellationToken)
    {
        var managerRequest = new ManagerRequest<int>
        {
            UserId = User.Identity?.Name ?? HttpContext.TraceIdentifier,
            Data = id
        };
        var response = await _songManager.DeleteSongAsync(managerRequest, cancellationToken);
        return response.Success ? NoContent() : BadRequest(response.ErrorMessage);
    }
}