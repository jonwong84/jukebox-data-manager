using Grpc.Core;
using Jukebox.DataManager.Contracts;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Grpc.Album;
using ManagerContracts = Jukebox.DataManager.Contracts.DataContracts.Album;
using GrpcAlbum = Jukebox.DataManager.Grpc.Album;

namespace Jukebox.DataManager.Grpc.Services;

public class AlbumServiceImpl : AlbumService.AlbumServiceBase
{
    private readonly IAlbumManager _albumManager;
    private readonly ILogger<AlbumServiceImpl> _logger;

    public AlbumServiceImpl(IAlbumManager albumManager, ILogger<AlbumServiceImpl> logger)
    {
        _albumManager = albumManager;
        _logger = logger;
    }

    public override async Task<GetAlbumResponse> GetAlbum(GetAlbumRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<int>
        {
            UserId = request.UserId,
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
            UserId = request.UserId,
            Data = new ManagerContracts.AddAlbumRequest
            {
                Title = request.Title,
                ArtistIds = request.ArtistIds.ToList(),
                ReleaseDate = request.ReleaseDate == string.Empty ? null : DateTime.Parse(request.ReleaseDate),
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

        // Fetch full details to return in response
        var getRequest = new ManagerRequest<int>
        {
            UserId = request.UserId,
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
            UserId = request.UserId,
            Data = new ManagerContracts.UpdateAlbumRequest
            {
                Id = request.Id,
                Title = request.Title,
                ArtistIds = request.ArtistIds.ToList(),
                ReleaseDate = request.ReleaseDate == string.Empty ? null : DateTime.Parse(request.ReleaseDate),
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
            UserId = request.UserId,
            Data = request.Id
        };

        var response = await _albumManager.DeleteAlbumAsync(managerRequest, context.CancellationToken);

        return new Jukebox.DataManager.Grpc.Common.DeleteResponse
        {
            Success = response.Success,
            ErrorMessage = response.ErrorMessage ?? string.Empty,
        };
    }

    private static GrpcAlbum.AlbumDetails MapToAlbumDetails(
        ManagerContracts.AlbumDetails album)
    {
        var details = new GrpcAlbum.AlbumDetails
        {
            Id = album.Id,
            Title = album.Title,
            CreatedAt = album.CreatedAt.ToString("O"),
            IsCompilation = album.IsCompilation,
            Description = album.Description,
            Artists =
            {
                album.Artists.Select(a => new Jukebox.DataManager.Grpc.Common.ArtistSummary
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
}