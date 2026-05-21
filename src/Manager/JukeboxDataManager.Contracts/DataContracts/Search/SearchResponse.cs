using Jukebox.DataManager.Contracts.DataContracts.Song;

namespace Jukebox.DataManager.Contracts.DataContracts.Search
{
    public class SearchResponse
    {
        public List<SongSummary> Songs { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
