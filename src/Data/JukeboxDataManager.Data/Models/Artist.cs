namespace JukeboxDataManager.Data.Models;

public class Artist
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Song> Songs { get; set; } = new List<Song>();
    public ICollection<Album> Albums { get; set; } = new List<Album>();
}
