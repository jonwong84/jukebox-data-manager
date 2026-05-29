using Grpc.Core;
using Jukebox.DataManager.Contracts;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Grpc.Artist;
using Jukebox.DataManager.Grpc.Common;
using GrpcArtist = Jukebox.DataManager.Grpc.Artist;
using ManagerContracts = Jukebox.DataManager.Contracts.DataContracts.Artist;

namespace Jukebox.DataManager.Grpc.Services;

public class ArtistServiceImpl : ArtistService.ArtistServiceBase
{
    private readonly IArtistManager _artistManager;
    private readonly ILogger<ArtistServiceImpl> _logger;

    public ArtistServiceImpl(IArtistManager artistManager, ILogger<ArtistServiceImpl> logger)
    {
        _artistManager = artistManager;
        _logger = logger;
    }

    public override async Task<GetArtistResponse> GetArtist(GetArtistRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<int>
        {
            UserId = request.UserId,
            Data = request.Id
        };

        var response = await _artistManager.FindByIdAsync(managerRequest, context.CancellationToken);

        if (!response.Success)
        {
            return new GetArtistResponse
            {
                Success = false,
                ErrorMessage = response.ErrorMessage ?? string.Empty,
            };
        }

        return new GetArtistResponse
        {
            Success = true,
            Artist = MapToArtistDetails(response.Data!),
        };
    }

    public override async Task<ArtistResponse> CreateArtist(CreateArtistRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<ManagerContracts.AddArtistRequest>
        {
            UserId = request.UserId,
            Data = new ManagerContracts.AddArtistRequest
            {
                Name = request.Name,
                Bio = request.Bio,
            }
        };

        var response = await _artistManager.AddArtistAsync(managerRequest, context.CancellationToken);

        if (!response.Success)
        {
            return new ArtistResponse
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

        var getResponse = await _artistManager.FindByIdAsync(getRequest, context.CancellationToken);

        if (!getResponse.Success)
        {
            return new ArtistResponse
            {
                Success = false,
                ErrorMessage = getResponse.ErrorMessage ?? string.Empty,
            };
        }

        return new ArtistResponse
        {
            Success = true,
            Artist = MapToArtistDetails(getResponse.Data!),
        };
    }

    public override async Task<ArtistResponse> UpdateArtist(UpdateArtistRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<ManagerContracts.UpdateArtistRequest>
        {
            UserId = request.UserId,
            Data = new ManagerContracts.UpdateArtistRequest
            {
                Id = request.Id,
                Name = request.Name,
                Bio = request.Bio,
            }
        };

        var response = await _artistManager.UpdateArtistAsync(managerRequest, context.CancellationToken);

        if (!response.Success)
        {
            return new ArtistResponse
            {
                Success = false,
                ErrorMessage = response.ErrorMessage ?? string.Empty,
            };
        }

        return new ArtistResponse
        {
            Success = true,
            Artist = MapToArtistDetails(response.Data!),
        };
    }

    public override async Task<Jukebox.DataManager.Grpc.Common.DeleteResponse> DeleteArtist(DeleteArtistRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<int>
        {
            UserId = request.UserId,
            Data = request.Id
        };

        var response = await _artistManager.DeleteArtistAsync(managerRequest, context.CancellationToken);

        return new Jukebox.DataManager.Grpc.Common.DeleteResponse
        {
            Success = response.Success,
            ErrorMessage = response.ErrorMessage ?? string.Empty,
        };
    }

    public override async Task<ListArtistsResponse> ListArtists(ListArtistsRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<ManagerContracts.ListArtistsRequest>
        {
            UserId = request.UserId,
            Data = new ManagerContracts.ListArtistsRequest
            {
                PageNumber = request.Pagination.Page,
                PageSize = request.Pagination.PageSize,
                NameSearch = request.HasNameSearch ? request.NameSearch : null
            }
        };

        var result = await _artistManager.ListAsync(managerRequest, context.CancellationToken);

        if (!result.Success)
            throw new RpcException(new Status(StatusCode.Internal, result.ErrorMessage ?? "List failed"));

        var response = new ListArtistsResponse
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

        response.Artists.AddRange(result.Data.Items.Select(a => new Common.ArtistSummary
        {
            Id = a.Id,
            Name = a.Name
        }));

        return response;
    }

    private static GrpcArtist.ArtistDetails MapToArtistDetails(
        ManagerContracts.ArtistDetails artist) =>
        new()
        {
            Id = artist.Id,
            Name = artist.Name,
            Bio = artist.Bio,
            CreatedAt = artist.CreatedAt.ToString("O"),
            Albums =
            {
                artist.Albums.Select(a => new Jukebox.DataManager.Grpc.Common.AlbumSummary
                {
                    Id = a.Id,
                    Title = a.Title,
                    Artists =
                    {
                        a.Artists.Select(ar => new Jukebox.DataManager.Grpc.Common.ArtistSummary
                        {
                            Id = ar.Id,
                            Name = ar.Name,
                        })
                    },
                })
            },
        };
}