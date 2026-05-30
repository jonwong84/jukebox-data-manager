using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Search;

namespace Jukebox.DataManager.Managers.Interfaces
{
    public interface ISongSearchManager
    {
        Task<SearchResponse> GetSongsAsync(ManagerRequest<SearchRequest> managerRequest);
    }
}
