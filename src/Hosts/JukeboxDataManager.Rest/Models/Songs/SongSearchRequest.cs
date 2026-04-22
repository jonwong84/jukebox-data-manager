namespace JukeboxDataManager.Rest.Models.Songs;

public class SongSearchRequest
{
    public string? Title { get; set; }
    public int? ArtistId { get; set; } = 0;
    public int? AlbumId { get; set; } = 0;
    public List<int>? GenreIds { get; set; } = new();
}
