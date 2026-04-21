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
        _context.Entry(song).State = EntityState.Modified;
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Songs.Any(s => s.Id == id)) return NotFound();
            throw;
        }
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
