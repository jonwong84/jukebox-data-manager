using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Search;

namespace Jukebox.DataManager.Contracts
{
    public interface ISongSearchManager
    {
        Task<SearchResponse> GetSongsAsync(ManagerRequest<SearchRequest> managerRequest);
    }
}
