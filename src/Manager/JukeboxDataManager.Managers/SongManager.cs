using JukeboxDataManager.Contracts;
using JukeboxDataManager.Contracts.SongSummary;
using System.Threading;
using System.Threading.Tasks;

namespace JukeboxDataManager.Managers;

public class SongManager : ISongManager
{
    public Task<SongSearchResponse> SearchSongsAsync(SongSearchRequest request, CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual search logic
        return Task.FromResult(new SongSearchResponse());
    }
}
