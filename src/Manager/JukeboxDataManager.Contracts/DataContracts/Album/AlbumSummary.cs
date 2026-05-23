using Jukebox.DataManager.Contracts.DataContracts.Artist;

namespace Jukebox.DataManager.Contracts.DataContracts.Album
{
    public class AlbumSummary
    {
        public int Id { get; set; }
        public required string Title { get; set; } = String.Empty;
        public List<ArtistSummary> Artists { get; set; } = new List<ArtistSummary>();
    }
}
