namespace JukeboxDataManager.Data.Models;

public class Genre
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<SongGenre> SongGenres { get; set; } = new List<SongGenre>();
}
