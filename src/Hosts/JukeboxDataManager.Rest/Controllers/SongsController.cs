using Jukebox.DataManager.Contracts;
using Jukebox.DataManager.Contracts.DataContracts.Song;
using Microsoft.AspNetCore.Mvc;
using Jukebox.DataManager.Contracts.DataContracts.Common;


namespace Jukebox.DataManager.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]

public class SongsController(ISongManager songManager) : ControllerBase
{
    private readonly ISongManager _songManager = songManager;

    [HttpGet("{id}")]
    public async Task<ActionResult<SongDetails>> GetSong(int id)
    {
        var response = await _songManager.FindByIdAsync(id);
        return response.Success ? Ok(response.Data) : NotFound(response.ErrorMessage);
    }

    [HttpPost]
    public async Task<ActionResult<SongDetails>> AddSong(AddSongRequest request)
    {
        var managerRequest = new ManagerRequest<AddSongRequest>(){
            UserId = User.Identity?.Name ?? "Unknown",
            Data = request
        };
        var response = await _songManager.AddSongAsync(managerRequest);
        return response.Success ? CreatedAtAction(nameof(GetSong), new { id = response.Data.Id }, response.Data) : BadRequest(response.ErrorMessage);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SongSummary>> UpdateSong(int id, UpdateSongRequest request)
    {
        var managerRequest = new ManagerRequest<UpdateSongRequest>(){
            UserId = User.Identity?.Name ?? "Unknown",
            Data = request
        };
        var response = await _songManager.UpdateSongAsync(managerRequest);
        return response.Success ? Ok(response.Data) : BadRequest(response.ErrorMessage);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<DeleteSongResult>> DeleteSong(int id)
    {
        var managerRequest = new ManagerRequest<int>(){
            UserId = User.Identity?.Name ?? "Unknown",
            Data = id
        };
        var response = await _songManager.DeleteSongAsync(managerRequest);
        return response.Success ? NoContent() : BadRequest(response.ErrorMessage);
    }
}
