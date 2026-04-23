using JukeboxDataManager.Contracts.SongSummary;

namespace JukeboxDataManager.Contracts;

public interface ISongManager
{
    Task<SongSearchResponse> SearchSongsAsync(SongSearchRequest request, CancellationToken cancellationToken = default);
}
