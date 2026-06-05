using Grpc.Core;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Grpc.Album;
using Jukebox.DataManager.Grpc.Artist;
using Jukebox.DataManager.Grpc.Common;
using Jukebox.DataManager.Managers.Interfaces;
using System.Globalization;
using GrpcAlbum = Jukebox.DataManager.Grpc.Album;
using ManagerContracts = Jukebox.DataManager.Contracts.DataContracts.Album;

namespace Jukebox.DataManager.Grpc.Services;

public class AlbumServiceImpl : AlbumService.AlbumServiceBase
{
    private readonly IAlbumManager _albumManager;

    public AlbumServiceImpl(IAlbumManager albumManager)
    {
        _albumManager = albumManager;
    }

    public override async Task<GetAlbumResponse> GetAlbum(GetAlbumRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<int>
        {
            UserId = GetUserId(context),
            Data = request.Id
        };

        var response = await _albumManager.FindByIdAsync(managerRequest, context.CancellationToken);

        if (!response.Success)
        {
            return new GetAlbumResponse
            {
                Success = false,
                ErrorMessage = response.ErrorMessage ?? string.Empty,
            };
        }

        return new GetAlbumResponse
        {
            Success = true,
            Album = MapToAlbumDetails(response.Data!),
        };
    }

    public override async Task<AlbumResponse> CreateAlbum(CreateAlbumRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<ManagerContracts.AddAlbumRequest>
        {
            UserId = GetUserId(context),
            Data = new ManagerContracts.AddAlbumRequest
            {
                Title = request.Title,
                ArtistIds = request.ArtistIds.ToList(),
                ReleaseDate = request.ReleaseDate == string.Empty ? null : DateTime.Parse(request.ReleaseDate, CultureInfo.InvariantCulture),
                IsCompilation = request.IsCompilation,
                Description = request.Description,
            }
        };

        var response = await _albumManager.AddAlbumAsync(managerRequest, context.CancellationToken);

        if (!response.Success)
        {
            return new AlbumResponse
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

        var getResponse = await _albumManager.FindByIdAsync(getRequest, context.CancellationToken);

        if (!getResponse.Success)
        {
            return new AlbumResponse
            {
                Success = false,
                ErrorMessage = getResponse.ErrorMessage ?? string.Empty,
            };
        }

        return new AlbumResponse
        {
            Success = true,
            Album = MapToAlbumDetails(getResponse.Data!),
        };
    }

    public override async Task<AlbumResponse> UpdateAlbum(UpdateAlbumRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<ManagerContracts.UpdateAlbumRequest>
        {
            UserId = GetUserId(context),
            Data = new ManagerContracts.UpdateAlbumRequest
            {
                Id = request.Id,
                Title = request.Title,
                ArtistIds = request.ArtistIds.ToList(),
                ReleaseDate = request.ReleaseDate == string.Empty ? null : DateTime.Parse(request.ReleaseDate, CultureInfo.InvariantCulture),
                IsCompilation = request.IsCompilation,
                Description = request.Description,
            }
        };

        var response = await _albumManager.UpdateAlbumAsync(managerRequest, context.CancellationToken);

        if (!response.Success)
        {
            return new AlbumResponse
            {
                Success = false,
                ErrorMessage = response.ErrorMessage ?? string.Empty,
            };
        }

        return new AlbumResponse
        {
            Success = true,
            Album = MapToAlbumDetails(response.Data!),
        };
    }

    public override async Task<Jukebox.DataManager.Grpc.Common.DeleteResponse> DeleteAlbum(DeleteAlbumRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<int>
        {
            UserId = GetUserId(context),
            Data = request.Id
        };

        var response = await _albumManager.DeleteAlbumAsync(managerRequest, context.CancellationToken);

        return new Jukebox.DataManager.Grpc.Common.DeleteResponse
        {
            Success = response.Success,
            ErrorMessage = response.ErrorMessage ?? string.Empty,
        };
    }

    public override async Task<ListAlbumsResponse> ListAlbums(ListAlbumsRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<ManagerContracts.ListAlbumsRequest>
        {
            UserId = GetUserId(context),
            Data = new ManagerContracts.ListAlbumsRequest
            {
                PageNumber = request.Pagination?.Page ?? 1,
                PageSize = request.Pagination?.PageSize ?? 10,
                ArtistId = request.HasArtistId ? request.ArtistId : null,
                GenreId = request.HasGenreId ? request.GenreId : null,
                TitleSearch = request.HasTitleSearch ? request.TitleSearch : null
            }
        };

        var result = await _albumManager.ListAsync(managerRequest, context.CancellationToken);

        if (!result.Success)
            throw new RpcException(new Status(StatusCode.Internal, result.ErrorMessage ?? "List failed"));

        var response = new ListAlbumsResponse
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

        response.Albums.AddRange(result.Data.Items.Select(a => new AlbumSummary
        {
            Id = a.Id,
            Title = a.Title,
            Artists = { a.Artists.Select(ar => new Artist.ArtistSummary { Id = ar.Id, Name = ar.Name }) }
        }));

        return response;
    }

    private static GrpcAlbum.AlbumDetails MapToAlbumDetails(
        ManagerContracts.AlbumDetails album)
    {
        var details = new GrpcAlbum.AlbumDetails
        {
            Id = album.Id,
            Title = album.Title,
            IsCompilation = album.IsCompilation,
            Description = album.Description,
            Artists =
            {
                album.Artists.Select(a => new Artist.ArtistSummary
                {
                    Id = a.Id,
                    Name = a.Name,
                })
            },
        };

        if (album.ReleaseDate.HasValue)
        {
            details.ReleaseDate = album.ReleaseDate.Value.ToString("O");
        }

        return details;
    }

    private static string GetUserId(ServerCallContext context) =>
        context.UserState.TryGetValue("userId", out var uid) ? uid as string ?? string.Empty : string.Empty;
}