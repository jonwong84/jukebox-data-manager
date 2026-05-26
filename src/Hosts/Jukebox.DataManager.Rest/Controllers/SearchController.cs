using Jukebox.DataManager.Contracts;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Search;
using Jukebox.DataManager.Contracts.DataContracts.Song;
using Jukebox.DataManager.Rest.Mapping;
using Microsoft.AspNetCore.Mvc;

namespace Jukebox.DataManager.Rest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
    }
}
