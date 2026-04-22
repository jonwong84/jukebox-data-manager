using JukeboxDataManager.Data;
using JukeboxDataManager.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JukeboxDataManager.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtistsController : ControllerBase
{
    private readonly JukeboxDbContext _context;

    public ArtistsController(JukeboxDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Artist>>> GetArtists()
    {
        return await _context.Artists.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Artist>> GetArtist(int id)
    {
        var artist = await _context.Artists.Include(a => a.Songs).Include(a => a.Albums).FirstOrDefaultAsync(a => a.Id == id);
        if (artist == null) return NotFound();
        return artist;
    }

    [HttpPost]
    public async Task<ActionResult<Artist>> CreateArtist(Artist artist)
    {
        artist.CreatedAt = DateTime.UtcNow;
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetArtist), new { id = artist.Id }, artist);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateArtist(int id, Artist artist)
    {
        if (id != artist.Id) return BadRequest();
        var existing = await _context.Artists.FindAsync(id);
        if (existing == null) return NotFound();
        existing.Name = artist.Name;
        existing.Bio = artist.Bio;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArtist(int id)
    {
        var artist = await _context.Artists.FindAsync(id);
        if (artist == null) return NotFound();
        _context.Artists.Remove(artist);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
