namespace Jukebox.DataManager.Contracts.Song;

public class SongSearchResponse
{
    public List<SongSummary> Songs { get; set; } = new();
    public int TotalCount { get; set; }
}
