using JukeboxDataManager.Data.Managers;
using JukeboxDataManager.Rest.Models.Songs;
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
        // TODO: Replace with manager/data access call
        var song = await _songManager.FindByIdAsync(id);
        if (song == null) return NotFound();
        var summary = new SongSummary
        {
            Id = song.Id,
            Title = song.Title,
            ArtistId = song.ArtistId,
            AlbumId = song.AlbumId,
            Duration = song.Duration
        };
        return summary;
    }

    [HttpPost]
    public async Task<ActionResult<Song>> AddSong(Song song)
    {
        song.CreatedAt = DateTime.UtcNow;
        _context.Songs.Add(song);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetSong), new { id = song.Id }, song);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSong(int id, Song song)
    {
        if (id != song.Id) return BadRequest();
        var existing = await _context.Songs.FindAsync(id);
        if (existing == null) return NotFound();
        existing.Title = song.Title;
        existing.ArtistId = song.ArtistId;
        existing.AlbumId = song.AlbumId;
        existing.Duration = song.Duration;
        existing.Genre = song.Genre;
        existing.TrackNumber = song.TrackNumber;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSong(int id)
    {
        var song = await _context.Songs.FindAsync(id);
        if (song == null) return NotFound();
        _context.Songs.Remove(song);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
