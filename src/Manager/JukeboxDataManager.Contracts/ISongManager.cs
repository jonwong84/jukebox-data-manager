using Jukebox.DataManager.Contracts.Song;

namespace Jukebox.DataManager.Contracts;

public interface ISongManager
{
    Task<SongSummary?> FindByIdAsync(int id);
    Task<SongSearchResponse> SearchSongsAsync(SongSearchRequest request, CancellationToken cancellationToken = default);

    Task<SongSummary> AddSongAsync(AddSongRequest request);

    Task<SongSummary> UpdateSongAsync(UpdateSongRequest request);

    Task<bool> DeleteSongAsync(int id);
}
