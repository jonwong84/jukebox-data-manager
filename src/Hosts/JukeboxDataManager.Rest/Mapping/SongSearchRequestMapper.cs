using Jukebox.DataManager.Contracts.DataContracts.Search;
using Jukebox.DataManager.Contracts.DataContracts.Song;

namespace Jukebox.DataManager.Rest.Mapping
{
    public static class SongSearchRequestMapper
    {
        public static SearchRequest MapSearchRequest(SongSearchRequest request)
        {
            return new SearchRequest
            {
                // TODO: Map properties from SongSearchRequest to SearchRequest; this will involve building out filters based on the search criteria provided in SongSearchRequest.
            };
        }
    }
}
