using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Song;

namespace Jukebox.DataManager.Contracts;

public interface ISongManager
{
    Task<ManagerResponse<SongDetails>> FindByIdAsync(ManagerRequest<int> managerRequest, CancellationToken cancellationToken = default);

    Task<ManagerResponse<SongSummary>> AddSongAsync(ManagerRequest<AddSongRequest> managerRequest, CancellationToken cancellationToken = default);

    Task<ManagerResponse<SongDetails>> UpdateSongAsync(ManagerRequest<UpdateSongRequest> managerRequest, CancellationToken cancellationToken = default);

    Task<ManagerResponse<bool>> DeleteSongAsync(ManagerRequest<int> managerRequest, CancellationToken cancellationToken = default);

    Task<ManagerResponse<PagedResult<SongSummary>>> ListAsync(ManagerRequest<ListSongsRequest> request, CancellationToken cancellationToken = default);
}
