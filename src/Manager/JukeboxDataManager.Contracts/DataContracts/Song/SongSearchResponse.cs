namespace Jukebox.DataManager.Contracts.DataContracts.Song;

public class SongSearchResponse
{
    public List<SongSummary> Songs { get; set; } = new();
    public int TotalCount { get; set; }
}
