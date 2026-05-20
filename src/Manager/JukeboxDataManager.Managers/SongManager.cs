using AutoMapper;
using Jukebox.DataManager.Contracts;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Song;

namespace Jukebox.DataManager.Managers;

public class SongManager : ISongManager
{
    private readonly IMapper _mapper;

    public SongManager(IMapper mapper)
    {
        _mapper = mapper;
    }

    public Task<ManagerResponse<SongSummary>> AddSongAsync(ManagerRequest<AddSongRequest> managerRequest)
    {
        throw new NotImplementedException();
    }

    public Task<ManagerResponse<bool>> DeleteSongAsync(ManagerRequest<int> managerRequest)
    {
        throw new NotImplementedException();
    }

    public Task<ManagerResponse<SongSummary>> FindByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<ManagerResponse<SongSummary>> UpdateSongAsync(ManagerRequest<UpdateSongRequest> managerRequest)
    {
        throw new NotImplementedException();
    }
}
