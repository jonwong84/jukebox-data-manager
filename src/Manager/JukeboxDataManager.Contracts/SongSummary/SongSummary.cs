namespace JukeboxDataManager.Contracts.SongSummary
{
    public class SongSummary
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ArtistId { get; set; }
        public int? AlbumId { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
