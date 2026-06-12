using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Genre;

namespace Jukebox.DataManager.Managers.Interfaces;

public interface IGenreManager
{
    Task<ManagerResponse<GenreDetails>> FindByIdAsync(ManagerRequest<int> managerRequest, CancellationToken cancellationToken = default);
    Task<ManagerResponse<GenreSummary>> AddGenreAsync(ManagerRequest<AddGenreRequest> managerRequest, CancellationToken cancellationToken = default);
    Task<ManagerResponse<GenreDetails>> UpdateGenreAsync(ManagerRequest<UpdateGenreRequest> managerRequest, CancellationToken cancellationToken = default);
    Task<ManagerResponse<bool>> DeleteGenreAsync(ManagerRequest<int> managerRequest, CancellationToken cancellationToken = default);
    Task<ManagerResponse<PagedResult<GenreSummary>>> ListAsync(ManagerRequest<ListGenresRequest> managerRequest, CancellationToken cancellationToken = default);
}