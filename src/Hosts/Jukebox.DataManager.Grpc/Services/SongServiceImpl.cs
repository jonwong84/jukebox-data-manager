using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Song;
using Jukebox.DataManager.Grpc.Common;
using Jukebox.DataManager.Grpc.Song;
using Jukebox.DataManager.Managers.Interfaces;
using GrpcSong = Jukebox.DataManager.Grpc.Song;
using ManagerContracts = Jukebox.DataManager.Contracts.DataContracts.Song;

namespace Jukebox.DataManager.Grpc.Services;

public class SongServiceImpl : SongService.SongServiceBase
{
    private readonly ISongManager _songManager;

    public SongServiceImpl(ISongManager songManager)
    {
        _songManager = songManager;
    }

    public override async Task<GetSongResponse> GetSong(GetSongRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<int>
        {
            UserId = GetUserId(context),
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
            UserId = GetUserId(context),
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

        var getRequest = new ManagerRequest<int>
        {
            UserId = GetUserId(context),
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
            UserId = GetUserId(context),
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

    public override async Task<Common.DeleteResponse> DeleteSong(DeleteSongRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<int>
        {
            UserId = GetUserId(context),
            Data = request.Id
        };

        var response = await _songManager.DeleteSongAsync(managerRequest, context.CancellationToken);

        return new Jukebox.DataManager.Grpc.Common.DeleteResponse
        {
            Success = response.Success,
            ErrorMessage = response.ErrorMessage ?? string.Empty,
        };
    }

    public override async Task<GrpcSong.ListSongsResponse> ListSongs(GrpcSong.ListSongsRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<ManagerContracts.ListSongsRequest>
        {
            UserId = GetUserId(context),
            Data = new ManagerContracts.ListSongsRequest
            {
                PageNumber = request.Pagination?.Page ?? 1,
                PageSize = request.Pagination?.PageSize ?? 10,
                ArtistId = request.HasArtistId ? request.ArtistId : null,
                AlbumId = request.HasAlbumId ? request.AlbumId : null,
                GenreId = request.HasGenreId ? request.GenreId : null,
                MinBpm = request.HasMinBpm ? request.MinBpm : null,
                MaxBpm = request.HasMaxBpm ? request.MaxBpm : null,
                TitleSearch = request.HasTitleSearch ? request.TitleSearch : null
            }
        };

        var result = await _songManager.ListAsync(managerRequest, context.CancellationToken);

        if (!result.Success)
            throw new RpcException(new Status(StatusCode.Internal, result.ErrorMessage ?? "List failed"));

        var response = new ListSongsResponse
        {
            Success = true,
            Pagination = new PaginationResponse
            {
                TotalCount = result.Data!.TotalCount,
                Page = result.Data.Page,
                PageSize = result.Data.PageSize,
                TotalPages = (int)Math.Ceiling((double)result.Data.TotalCount / result.Data.PageSize)
            }
        };

        response.Songs.AddRange(result.Data.Items.Select(s => new GrpcSong.SongSummary
        {
            Id = s.Id,
            Title = s.Title,
            Artist = s.Artist,
            Album = s.Album
        }));

        return response;
    }

    private static GrpcSong.SongDetails MapToSongDetails(
        ManagerContracts.SongDetails song)
    {
        var details = new GrpcSong.SongDetails
        {
            Id = song.Id,
            Title = song.Title,
            ArtistId = song.ArtistId,
            Artist = new Jukebox.DataManager.Grpc.Common.ArtistSummary
            {
                Id = song.Artist.Id,
                Name = song.Artist.Name,
            },
            DurationTicks = song.Duration.Ticks,
            Genres = { song.Genres.Select(g => new Jukebox.DataManager.Grpc.Common.GenreSummary
            {
                Id = g.Id,
                Name = g.Name,
            })},
            Lyrics = song.Lyrics,
        };

        if (song.AlbumId.HasValue)
        {
            details.AlbumId = song.AlbumId.Value;
        }

        if (song.Album is not null)
        {
            details.Album = new Jukebox.DataManager.Grpc.Common.AlbumSummary
            {
                Id = song.Album.Id,
                Title = song.Album.Title,
                Artists = { song.Album.Artists.Select(a => new Jukebox.DataManager.Grpc.Common.ArtistSummary
                {
                    Id = a.Id,
                    Name = a.Name,
                })},
            };
        }

        if (song.TrackNumber.HasValue)
        {
            details.TrackNumber = song.TrackNumber.Value;
        }

        if (song.Bpm.HasValue)
        {
            details.Bpm = song.Bpm.Value;
        }

        return details;
    }

    private static string GetUserId(ServerCallContext context) =>
        context.UserState.TryGetValue("userId", out var uid) ? uid as string ?? string.Empty : string.Empty;
}