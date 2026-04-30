using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Song;

namespace Jukebox.DataManager.Contracts;

public interface ISongManager
{
    Task<ManagerResponse<SongSummary>> FindByIdAsync(int id);

    Task<ManagerResponse<SongSummary>> AddSongAsync(ManagerRequest<AddSongRequest> managerRequest);

    Task<ManagerResponse<SongSummary>> UpdateSongAsync(ManagerRequest<UpdateSongRequest> managerRequest);

    Task<ManagerResponse<bool>> DeleteSongAsync(ManagerRequest<int> managerRequest);
}
