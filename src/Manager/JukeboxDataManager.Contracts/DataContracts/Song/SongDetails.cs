namespace Jukebox.DataManager.Contracts.DataContracts.Song
{
    public class SongDetails
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public int ArtistId { get; set; }
        public Artist Artist { get; set; } = null!;
        public int? AlbumId { get; set; }
        public Album? Album { get; set; }
        public TimeSpan Duration { get; set; }
        public ICollection<SongGenre> SongGenres { get; set; } = new List<SongGenre>();
        public int? TrackNumber { get; set; }
        public int? Bpm { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public SongLyrics? Lyrics { get; set; }
    }
}
