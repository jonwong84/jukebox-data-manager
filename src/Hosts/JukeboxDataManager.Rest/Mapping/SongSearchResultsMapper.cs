using Jukebox.DataManager.Contracts.DataContracts.Search;
using Jukebox.DataManager.Contracts.DataContracts.Song;

namespace Jukebox.DataManager.Rest.Mapping
{
    public static class SongSearchResultsMapper
    {
        public static SongSearchResponse MapSearchResults(SearchResponse response)
        {
            return new SongSearchResponse
            {
            };
        }
    }
}
