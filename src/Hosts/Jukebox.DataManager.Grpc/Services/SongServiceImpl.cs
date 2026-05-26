using Grpc.Core;
using Jukebox.DataManager.Contracts;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Song;
using Jukebox.DataManager.Grpc.Song;
using Google.Protobuf.WellKnownTypes;
using GrpcSong = Jukebox.DataManager.Grpc.Song;
using ManagerContracts = Jukebox.DataManager.Contracts.DataContracts.Song;

namespace Jukebox.DataManager.Grpc.Services;

public class SongServiceImpl : SongService.SongServiceBase
{
    private readonly ISongManager _songManager;
    private readonly ILogger<SongServiceImpl> _logger;

    public SongServiceImpl(ISongManager songManager, ILogger<SongServiceImpl> logger)
    {
        _songManager = songManager;
        _logger = logger;
    }

    public override async Task<GetSongResponse> GetSong(GetSongRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<int>
        {
            UserId = request.UserId,
            Data = request.Id
        };

        var response = await _songManager.FindByIdAsync(managerRequest, context.CancellationToken);

        if (!response.Success)
        {
            return new GetSongResponse
            {
                Success = false,
                ErrorMessage = response.ErrorMessage ?? string.Empty,
            };
        }

        return new GetSongResponse
        {
            Success = true,
            Song = MapToSongDetails(response.Data!),
        };
    }

    public override async Task<SongResponse> CreateSong(CreateSongRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<AddSongRequest>
        {
            UserId = request.UserId,
            Data = new AddSongRequest
            {
                Title = request.Title,
                ArtistId = request.ArtistId,
                AlbumId = request.HasAlbumId ? request.AlbumId : null,
                Duration = TimeSpan.FromTicks(request.DurationTicks),
                GenreIds = request.GenreIds.ToList(),
                TrackNumber = request.HasTrackNumber ? request.TrackNumber : null,
                Bpm = request.HasBpm ? request.Bpm : null,
                Lyrics = request.Lyrics,
            }
        };

        var response = await _songManager.AddSongAsync(managerRequest, context.CancellationToken);

        if (!response.Success)
        {
            return new SongResponse
            {
                Success = false,
                ErrorMessage = response.ErrorMessage ?? string.Empty,
            };
        }

        // Fetch full details to return in response
        var getRequest = new ManagerRequest<int>
        {
            UserId = request.UserId,
            Data = response.Data!.Id
        };

        var getResponse = await _songManager.FindByIdAsync(getRequest, context.CancellationToken);

        if (!getResponse.Success)
        {
            return new SongResponse
            {
                Success = false,
                ErrorMessage = getResponse.ErrorMessage ?? string.Empty,
            };
        }

        return new SongResponse
        {
            Success = true,
            Song = MapToSongDetails(getResponse.Data!),
        };
    }

    public override async Task<SongResponse> UpdateSong(GrpcSong.UpdateSongRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<ManagerContracts.UpdateSongRequest>
        {
            UserId = request.UserId,
            Data = new ManagerContracts.UpdateSongRequest
            {
                Id = request.Id,
                Title = request.Title,
                ArtistId = request.ArtistId,
                AlbumId = request.HasAlbumId ? request.AlbumId : null,
                Duration = TimeSpan.FromTicks(request.DurationTicks),
                GenreIds = request.GenreIds.ToList(),
                TrackNumber = request.HasTrackNumber ? request.TrackNumber : null,
                Bpm = request.HasBpm ? request.Bpm : null,
                Lyrics = request.Lyrics,
            }
        };

        var response = await _songManager.UpdateSongAsync(managerRequest, context.CancellationToken);

        if (!response.Success)
        {
            return new SongResponse
            {
                Success = false,
                ErrorMessage = response.ErrorMessage ?? string.Empty,
            };
        }

        return new SongResponse
        {
            Success = true,
            Song = MapToSongDetails(response.Data!),
        };
    }

    public override async Task<Jukebox.DataManager.Grpc.Common.DeleteResponse> DeleteSong(DeleteSongRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<int>
        {
            UserId = request.UserId,
            Data = request.Id
        };

        var response = await _songManager.DeleteSongAsync(managerRequest, context.CancellationToken);

        return new Jukebox.DataManager.Grpc.Common.DeleteResponse
        {
            Success = response.Success,
            ErrorMessage = response.ErrorMessage ?? string.Empty,
        };
    }

    private static GrpcSong.SongDetails MapToSongDetails(
        ManagerContracts.SongDetails song) => new()
        {
            Id = song.Id,
            Title = song.Title,
            ArtistId = song.ArtistId,
            Artist = new Jukebox.DataManager.Grpc.Common.ArtistSummary
            {
                Id = song.Artist.Id,
                Name = song.Artist.Name,
            },
            AlbumId = song.AlbumId ?? 0,
            Album = song.Album is null ? null : new Jukebox.DataManager.Grpc.Common.AlbumSummary
            {
                Id = song.Album.Id,
                Title = song.Album.Title,
                Artists = { song.Album.Artists.Select(a => new Jukebox.DataManager.Grpc.Common.ArtistSummary
                {
                    Id = a.Id,
                    Name = a.Name,
                })},
            },
            DurationTicks = song.Duration.Ticks,
            Genres = { song.Genres.Select(g => new Jukebox.DataManager.Grpc.Common.GenreSummary
            {
                Id = g.Id,
                Name = g.Name,
            })},
            TrackNumber = song.TrackNumber ?? 0,
            Bpm = song.Bpm ?? 0,
            Lyrics = song.Lyrics,
        };
}