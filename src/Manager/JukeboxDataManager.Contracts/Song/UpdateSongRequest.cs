namespace JukeboxDataManager.Contracts.Song;

public class UpdateSongRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ArtistId { get; set; }
    public int? AlbumId { get; set; }
    public TimeSpan Duration { get; set; }
    public int? TrackNumber { get; set; }
    // Add other fields as needed
}
