using Jukebox.DataManager.Contracts.DataContracts.Artist;

namespace Jukebox.DataManager.Contracts.DataContracts.Album;

public class AlbumDetails
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public List<ArtistSummary> Artists { get; set; } = [];
    public DateTime? ReleaseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsCompilation { get; set; }
    public string Description { get; set; } = string.Empty;
}