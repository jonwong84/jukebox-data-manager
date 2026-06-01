using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Search;
using Jukebox.DataManager.Contracts.DataContracts.Song;
using Jukebox.DataManager.Managers.Interfaces;
using Jukebox.DataManager.Rest.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jukebox.DataManager.Rest.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISongSearchManager _songSearchManager;

        public SearchController(ISongSearchManager songSearchManager)
        {
            _songSearchManager = songSearchManager;
        }

        [HttpPost("search")]
        public async Task<ActionResult<SearchResponse>> SearchSongs([FromBody] SearchRequest request)
        {
            var managerRequestData = SongSearchRequestMapper.MapSearchRequest(request);
            var managerRequest = new ManagerRequest<SearchRequest>
            {
                UserId = User.Identity?.Name ?? HttpContext.TraceIdentifier,
                Data = managerRequestData,
                RequestTime = DateTime.Now,
            };

            var resultsFromManager = await _songSearchManager.GetSongsAsync(managerRequest);
            var actionResult = SongSearchResultsMapper.MapSearchResults(resultsFromManager);
            return actionResult;
        }

        private string GetUserId() =>
    User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }
}
