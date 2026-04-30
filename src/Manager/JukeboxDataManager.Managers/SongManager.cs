using AutoMapper;
using Jukebox.DataManager.Contracts;
using Jukebox.DataManager.Contracts.DataContracts.Song;

namespace Jukebox.DataManager.Managers;

public class SongManager : ISongManager
{
    private readonly IMapper _mapper;

    public SongManager(IMapper mapper)
    {
        _mapper = mapper;
    }

    public Task<SongSummary> AddSongAsync(AddSongRequest song)
    {
        throw new NotImplementedException();
    }

    public Task<SongSummary?> FindByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<SongSummary> UpdateSongAsync(UpdateSongRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteSongAsync(int id)
    {
        throw new NotImplementedException();
    }
}
