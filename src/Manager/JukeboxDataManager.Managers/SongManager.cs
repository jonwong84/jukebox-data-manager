using AutoMapper;
using Jukebox.DataManager.Contracts;
using Jukebox.DataManager.Contracts.Song;

namespace Jukebox.DataManager.Managers;

public class SongManager : ISongManager
{
    private readonly IMapper _mapper;

    public SongManager(IMapper mapper)
    {
        _mapper = mapper;
    }

    public Task<SongSearchResponse> SearchSongsAsync(SongSearchRequest request, CancellationToken cancellationToken = default)
    {
        var response = new SongSearchResponse
        {
            Songs = new List<SongSummary>
            {
                new SongSummary { Id = 1, Title = "Imagine", Artist = "John Lennon", Album = "Imagine", Duration = TimeSpan.FromMinutes(3.1), Lyrics = "Imagine all the people..." },
                new SongSummary { Id = 2, Title = "Bohemian Rhapsody", Artist = "Queen", Album = "A Night at the Opera", Duration = TimeSpan.FromMinutes(5.55), Lyrics = "Is this the real life? Is this just fantasy?" }
            },
            TotalCount = 2
        };

        // TODO: Implement actual search logic
        return Task.FromResult(response);
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
