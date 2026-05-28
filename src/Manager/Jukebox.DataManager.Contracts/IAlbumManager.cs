using Jukebox.DataManager.Contracts.DataContracts.Album;
using Jukebox.DataManager.Contracts.DataContracts.Common;

namespace Jukebox.DataManager.Contracts;

public interface IAlbumManager
{
    Task<ManagerResponse<AlbumDetails>> FindByIdAsync(ManagerRequest<int> managerRequest, CancellationToken cancellationToken = default);

    Task<ManagerResponse<AlbumSummary>> AddAlbumAsync(ManagerRequest<AddAlbumRequest> managerRequest, CancellationToken cancellationToken = default);

    Task<ManagerResponse<AlbumDetails>> UpdateAlbumAsync(ManagerRequest<UpdateAlbumRequest> managerRequest, CancellationToken cancellationToken = default);

    Task<ManagerResponse<bool>> DeleteAlbumAsync(ManagerRequest<int> managerRequest, CancellationToken cancellationToken = default);
}