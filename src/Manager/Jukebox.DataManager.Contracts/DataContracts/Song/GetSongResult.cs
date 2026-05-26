namespace Jukebox.DataManager.Contracts.DataContracts.Song
{
    public class GetSongResult
    {
        public bool Success { get; set; }
        public SongSummary? SongSummary { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
