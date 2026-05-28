using Jukebox.DataManager.Contracts.DataContracts.Album;

namespace Jukebox.DataManager.Contracts.DataContracts.Artist;

public class ArtistDetails
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Bio { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<AlbumSummary> Albums { get; set; } = [];
}