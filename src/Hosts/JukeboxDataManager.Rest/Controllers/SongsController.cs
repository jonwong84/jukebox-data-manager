using JukeboxDataManager.Contracts;
using JukeboxDataManager.Contracts.Song;
using Microsoft.AspNetCore.Mvc;


namespace JukeboxDataManager.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]

public class SongsController : ControllerBase
{
    private readonly ISongManager _songManager;

    public SongsController(ISongManager songManager)
    {
        _songManager = songManager;
    }

    [HttpPost("search")]
    public async Task<ActionResult<SongSearchResponse>> SearchSongs([FromBody] SongSearchRequest request)
    {
        var response = await _songManager.SearchSongsAsync(request);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SongSummary>> GetSong(int id)
    {
        var song = await _songManager.FindByIdAsync(id); // song should be of type Song
        if (song == null) return NotFound();
        var summary = new SongSummary
        {
            Id = song.Id,
            Title = song.Title,
            Artist = song.Artist,
            Album = song.Album,
            Duration = song.Duration,
            Lyrics = song.Lyrics
        };
        return summary;
    }

    [HttpPost]
    public async Task<ActionResult<SongSummary>> AddSong(AddSongRequest request)
    {
        var summary = await _songManager.AddSongAsync(request);
        return CreatedAtAction(nameof(GetSong), new { id = summary.Id }, summary);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSong(UpdateSongRequest request)
    {
        throw new NotImplementedException("UpdateSong is not implemented yet.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSong(int id)
    {
        throw new NotImplementedException("DeleteSong is not implemented yet.");
    }
}
