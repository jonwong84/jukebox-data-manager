using Jukebox.DataManager.Contracts;
using Jukebox.DataManager.Contracts.DataContracts.Album;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AlbumsController : ControllerBase
{
    private readonly IAlbumManager _albumManager;
    private readonly ILogger<AlbumsController> _logger;

    public AlbumsController(IAlbumManager albumManager, ILogger<AlbumsController> logger)
    {
        _albumManager = albumManager;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAlbum(int id, CancellationToken cancellationToken)
    {
        var request = new ManagerRequest<int>
        {
            UserId = HttpContext.TraceIdentifier,
            Data = id
        };

        var result = await _albumManager.FindByIdAsync(request, cancellationToken);

        if (!result.Success)
            return NotFound(result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> ListAlbums([FromQuery] ListAlbumsRequest request, CancellationToken cancellationToken)
    {
        var managerRequest = new ManagerRequest<ListAlbumsRequest>
        {
            UserId = HttpContext.TraceIdentifier,
            Data = request
        };

        var result = await _albumManager.ListAsync(managerRequest, cancellationToken);

        if (!result.Success)
            return StatusCode(500, result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> AddAlbum([FromBody] AddAlbumRequest request, CancellationToken cancellationToken)
    {
        var managerRequest = new ManagerRequest<AddAlbumRequest>
        {
            UserId = HttpContext.TraceIdentifier,
            Data = request
        };

        var result = await _albumManager.AddAlbumAsync(managerRequest, cancellationToken);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return CreatedAtAction(nameof(GetAlbum), new { id = result.Data }, result.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAlbum(int id, [FromBody] UpdateAlbumRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest("Route id does not match request id");

        var managerRequest = new ManagerRequest<UpdateAlbumRequest>
        {
            UserId = HttpContext.TraceIdentifier,
            Data = request
        };

        var result = await _albumManager.UpdateAlbumAsync(managerRequest, cancellationToken);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAlbum(int id, CancellationToken cancellationToken)
    {
        var request = new ManagerRequest<int>
        {
            UserId = HttpContext.TraceIdentifier,
            Data = id
        };

        var result = await _albumManager.DeleteAlbumAsync(request, cancellationToken);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        return NoContent();
    }
}