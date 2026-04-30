namespace Jukebox.DataManager.Contracts.DataContracts.Song
{
    public class AddSongResponse
    {
        public bool Success { get; set; }
        public int? SongId { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
