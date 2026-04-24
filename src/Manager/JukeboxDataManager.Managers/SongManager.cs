using JukeboxDataManager.Contracts;
using JukeboxDataManager.Contracts.Song;

namespace JukeboxDataManager.Managers;

public class SongManager : ISongManager
{
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
        // TODO: Implement actual persistence logic
        var summary = new SongSummary();
        return Task.FromResult(summary);
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
