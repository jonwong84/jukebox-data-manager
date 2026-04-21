using JukeboxDataManager.Data;
using JukeboxDataManager.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JukeboxDataManager.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SongsController : ControllerBase
{
    private readonly JukeboxDbContext _context;

    public SongsController(JukeboxDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Song>>> GetSongs()
    {
        return await _context.Songs.Include(s => s.Artist).Include(s => s.Album).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Song>> GetSong(int id)
    {
        var song = await _context.Songs.Include(s => s.Artist).Include(s => s.Album).FirstOrDefaultAsync(s => s.Id == id);
        if (song == null) return NotFound();
        return song;
    }

    [HttpPost]
    public async Task<ActionResult<Song>> CreateSong(Song song)
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
