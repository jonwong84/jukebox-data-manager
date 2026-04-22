using JukeboxDataManager.Rest.Models;

namespace JukeboxDataManager.Data.Managers;

public interface ISongManager
{
    Task<SongSearchResponse> SearchSongsAsync(SongSearchRequest request, CancellationToken cancellationToken = default);
}
