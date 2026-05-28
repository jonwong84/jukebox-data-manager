using Jukebox.DataManager.Contracts.DataContracts.Artist;
using Jukebox.DataManager.Contracts.DataContracts.Common;

namespace Jukebox.DataManager.Contracts;

public interface IArtistManager
{
    Task<ManagerResponse<ArtistDetails>> FindByIdAsync(ManagerRequest<int> managerRequest, CancellationToken cancellationToken = default);

    Task<ManagerResponse<ArtistSummary>> AddArtistAsync(ManagerRequest<AddArtistRequest> managerRequest, CancellationToken cancellationToken = default);

    Task<ManagerResponse<ArtistDetails>> UpdateArtistAsync(ManagerRequest<UpdateArtistRequest> managerRequest, CancellationToken cancellationToken = default);

    Task<ManagerResponse<bool>> DeleteArtistAsync(ManagerRequest<int> managerRequest, CancellationToken cancellationToken = default);
}