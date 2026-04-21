using JukeboxDataManager.Data;
using JukeboxDataManager.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JukeboxDataManager.Rest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlbumsController : ControllerBase
{
    private readonly JukeboxDbContext _context;

    public AlbumsController(JukeboxDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Album>>> GetAlbums()
    {
        return await _context.Albums.Include(a => a.Artist).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Album>> GetAlbum(int id)
    {
        var album = await _context.Albums.Include(a => a.Artist).Include(a => a.Songs).FirstOrDefaultAsync(a => a.Id == id);
        if (album == null) return NotFound();
        return album;
    }

    [HttpPost]
    public async Task<ActionResult<Album>> CreateAlbum(Album album)
    {
        album.CreatedAt = DateTime.UtcNow;
        _context.Albums.Add(album);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAlbum), new { id = album.Id }, album);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAlbum(int id, Album album)
    {
        if (id != album.Id) return BadRequest();
        _context.Entry(album).State = EntityState.Modified;
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Albums.Any(a => a.Id == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAlbum(int id)
    {
        var album = await _context.Albums.FindAsync(id);
        if (album == null) return NotFound();
        _context.Albums.Remove(album);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
