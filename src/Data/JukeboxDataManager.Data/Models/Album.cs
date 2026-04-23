namespace JukeboxDataManager.Data.Models;

public class Album
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;
    public DateTime? ReleaseDate { get; set; }
    public ICollection<Song> Songs { get; set; } = new List<Song>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
