using Grpc.Core;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Genre;
using Jukebox.DataManager.Grpc.Genre;
using Jukebox.DataManager.Managers.Interfaces;
using GrpcGenre = Jukebox.DataManager.Grpc.Genre;
using ManagerContracts = Jukebox.DataManager.Contracts.DataContracts.Genre;

namespace Jukebox.DataManager.Grpc.Services;

public class GenreServiceImpl : GenreService.GenreServiceBase
{
    private readonly IGenreManager _genreManager;

    public GenreServiceImpl(IGenreManager genreManager)
    {
        _genreManager = genreManager;
    }

    public override async Task<GetGenreResponse> GetGenre(GetGenreRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<int>
        {
            UserId = GetUserId(context),
            Data = request.Id
        };

        var response = await _genreManager.FindByIdAsync(managerRequest, context.CancellationToken);

        if (!response.Success)
        {
            return new GetGenreResponse
            {
                Success = false,
                ErrorMessage = response.ErrorMessage ?? string.Empty,
            };
        }

        return new GetGenreResponse
        {
            Success = true,
            Genre = MapToGenreDetails(response.Data!),
        };
    }

    public override async Task<GenreResponse> CreateGenre(CreateGenreRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<AddGenreRequest>
        {
            UserId = GetUserId(context),
            Data = new AddGenreRequest
            {
                Name = request.Name,
                Description = request.Description,
                ParentGenreId = request.HasParentGenreId ? request.ParentGenreId : null,
            }
        };

        var response = await _genreManager.AddGenreAsync(managerRequest, context.CancellationToken);

        if (!response.Success)
        {
            return new GenreResponse
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

        var getResponse = await _genreManager.FindByIdAsync(getRequest, context.CancellationToken);

        if (!getResponse.Success)
        {
            return new GenreResponse
            {
                Success = false,
                ErrorMessage = getResponse.ErrorMessage ?? string.Empty,
            };
        }

        return new GenreResponse
        {
            Success = true,
            Genre = MapToGenreDetails(getResponse.Data!),
        };
    }

    public override async Task<GenreResponse> UpdateGenre(GrpcGenre.UpdateGenreRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<ManagerContracts.UpdateGenreRequest>
        {
            UserId = GetUserId(context),
            Data = new ManagerContracts.UpdateGenreRequest
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description,
                ParentGenreId = request.HasParentGenreId ? request.ParentGenreId : null,
            }
        };

        var response = await _genreManager.UpdateGenreAsync(managerRequest, context.CancellationToken);

        if (!response.Success)
        {
            return new GenreResponse
            {
                Success = false,
                ErrorMessage = response.ErrorMessage ?? string.Empty,
            };
        }

        return new GenreResponse
        {
            Success = true,
            Genre = MapToGenreDetails(response.Data!),
        };
    }

    public override async Task<Common.DeleteResponse> DeleteGenre(DeleteGenreRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<int>
        {
            UserId = GetUserId(context),
            Data = request.Id
        };

        var response = await _genreManager.DeleteGenreAsync(managerRequest, context.CancellationToken);

        return new Jukebox.DataManager.Grpc.Common.DeleteResponse
        {
            Success = response.Success,
            ErrorMessage = response.ErrorMessage ?? string.Empty,
        };
    }

    public override async Task<ListGenresResponse> ListGenres(GrpcGenre.ListGenresRequest request, ServerCallContext context)
    {
        var managerRequest = new ManagerRequest<ManagerContracts.ListGenresRequest>
        {
            UserId = GetUserId(context),
            Data = new ManagerContracts.ListGenresRequest
            {
                PageNumber = request.Pagination?.Page ?? 1,
                PageSize = request.Pagination?.PageSize ?? 20,
                NameSearch = request.HasNameSearch ? request.NameSearch : null,
                ParentGenreId = request.HasParentGenreId ? request.ParentGenreId : null,
            }
        };

        var result = await _genreManager.ListAsync(managerRequest, context.CancellationToken);

        if (!result.Success)
            throw new RpcException(new Status(StatusCode.Internal, result.ErrorMessage ?? "List failed"));

        var response = new ListGenresResponse
        {
            Success = true,
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationResponse
            {
                TotalCount = result.Data!.TotalCount,
                Page = result.Data.Page,
                PageSize = result.Data.PageSize,
                TotalPages = (int)Math.Ceiling((double)result.Data.TotalCount / result.Data.PageSize)
            }
        };

        response.Genres.AddRange(result.Data.Items.Select(g => new GrpcGenre.GenreSummary
        {
            Id = g.Id,
            Name = g.Name,
            ParentGenreId = g.ParentGenreId ?? 0,
        }));

        return response;
    }

    private static GrpcGenre.GenreDetails MapToGenreDetails(ManagerContracts.GenreDetails genre)
    {
        var details = new GrpcGenre.GenreDetails
        {
            Id = genre.Id,
            Name = genre.Name,
            Description = genre.Description,
        };

        if (genre.ParentGenreId.HasValue)
            details.ParentGenreId = genre.ParentGenreId.Value;

        if (genre.ParentGenre is not null)
        {
            details.ParentGenre = new GrpcGenre.GenreSummary
            {
                Id = genre.ParentGenre.Id,
                Name = genre.ParentGenre.Name,
                ParentGenreId = genre.ParentGenre.ParentGenreId ?? 0,
            };
        }

        details.SubGenres.AddRange(genre.SubGenres.Select(s => new GrpcGenre.GenreSummary
        {
            Id = s.Id,
            Name = s.Name,
            ParentGenreId = s.ParentGenreId ?? 0,
        }));

        return details;
    }

    private static string GetUserId(ServerCallContext context) =>
        context.UserState.TryGetValue("userId", out var uid) ? uid as string ?? string.Empty : string.Empty;
}