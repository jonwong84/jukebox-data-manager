using AutoMapper;
using Jukebox.DataAccess.Contracts.Interfaces;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Genre;
using Jukebox.DataManager.Managers.Interfaces;
using Microsoft.Extensions.Logging;
using DAL = Jukebox.DataAccess.Contracts.DataContracts;

namespace Jukebox.DataManager.Managers;

public sealed class GenreManager : IGenreManager
{
    private readonly IGenreRepositoryAccess _genreRepositoryAccess;
    private readonly IMapper _mapper;
    private readonly ILogger<GenreManager> _logger;

    public GenreManager(IGenreRepositoryAccess genreRepositoryAccess, IMapper mapper, ILogger<GenreManager> logger)
    {
        _genreRepositoryAccess = genreRepositoryAccess;
        _mapper = mapper;
        _logger = logger;
    }

    private static List<string> Validate(AddGenreRequest request)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("'Name' is required and cannot be empty or whitespace.");
        return errors;
    }

    private static List<string> Validate(UpdateGenreRequest request)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("'Name' is required and cannot be empty or whitespace.");
        return errors;
    }

    public async Task<ManagerResponse<GenreDetails>> FindByIdAsync(ManagerRequest<int> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} requested genre with ID {GenreId} at {RequestTime}",
            managerRequest.UserId, managerRequest.Data, managerRequest.RequestTime);

        var result = await _genreRepositoryAccess.GetByIdAsync(managerRequest.Data, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Genre with ID {GenreId} was not found. Requested by {UserId}",
                managerRequest.Data, managerRequest.UserId);

            return new ManagerResponse<GenreDetails>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        return new ManagerResponse<GenreDetails>
        {
            Success = true,
            Data = _mapper.Map<GenreDetails>(result.GenreDetails),
            ResponseTime = DateTime.UtcNow,
        };
    }

    public async Task<ManagerResponse<GenreSummary>> AddGenreAsync(ManagerRequest<AddGenreRequest> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} requested to add genre with name {Name} at {RequestTime}",
            managerRequest.UserId, managerRequest.Data.Name, managerRequest.RequestTime);

        var errors = Validate(managerRequest.Data);
        if (errors.Count > 0)
        {
            _logger.LogWarning("Validation failed for AddGenreAsync. Requested by {UserId}. Errors: {Errors}",
                managerRequest.UserId, string.Join(" ", errors));

            return new ManagerResponse<GenreSummary>
            {
                Success = false,
                ErrorMessage = string.Join(" ", errors),
                ResponseTime = DateTime.UtcNow,
            };
        }

        managerRequest.Data.UserId = managerRequest.UserId;
        var accessRequest = _mapper.Map<DAL.Genre.AddGenreRequest>(managerRequest.Data);
        var result = await _genreRepositoryAccess.AddAsync(accessRequest, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to add genre with name {Name}. Requested by {UserId}. Reason: {ErrorMessage}",
                managerRequest.Data.Name, managerRequest.UserId, result.ErrorMessage);

            return new ManagerResponse<GenreSummary>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        _logger.LogInformation("Successfully added genre with ID {GenreId}. Requested by {UserId}",
            result.GenreId, managerRequest.UserId);

        return new ManagerResponse<GenreSummary>
        {
            Success = true,
            Data = new GenreSummary { Id = result.GenreId },
            ResponseTime = DateTime.UtcNow,
        };
    }

    public async Task<ManagerResponse<GenreDetails>> UpdateGenreAsync(ManagerRequest<UpdateGenreRequest> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} requested to update genre with ID {GenreId} at {RequestTime}",
            managerRequest.UserId, managerRequest.Data.Id, managerRequest.RequestTime);

        var errors = Validate(managerRequest.Data);
        if (errors.Count > 0)
        {
            _logger.LogWarning("Validation failed for UpdateGenreAsync. Requested by {UserId}. Errors: {Errors}",
                managerRequest.UserId, string.Join(" ", errors));

            return new ManagerResponse<GenreDetails>
            {
                Success = false,
                ErrorMessage = string.Join(" ", errors),
                ResponseTime = DateTime.UtcNow,
            };
        }

        managerRequest.Data.UserId = managerRequest.UserId;
        var accessRequest = _mapper.Map<DAL.Genre.UpdateGenreRequest>(managerRequest.Data);
        var result = await _genreRepositoryAccess.UpdateAsync(accessRequest, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to update genre with ID {GenreId}. Requested by {UserId}. Reason: {ErrorMessage}",
                managerRequest.Data.Id, managerRequest.UserId, result.ErrorMessage);

            return new ManagerResponse<GenreDetails>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        _logger.LogInformation("Successfully updated genre with ID {GenreId}. Requested by {UserId}",
            managerRequest.Data.Id, managerRequest.UserId);

        return new ManagerResponse<GenreDetails>
        {
            Success = true,
            Data = _mapper.Map<GenreDetails>(result.GenreDetails),
            ResponseTime = DateTime.UtcNow,
        };
    }

    public async Task<ManagerResponse<bool>> DeleteGenreAsync(ManagerRequest<int> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} requested to delete genre with ID {GenreId} at {RequestTime}",
            managerRequest.UserId, managerRequest.Data, managerRequest.RequestTime);

        var result = await _genreRepositoryAccess.DeleteAsync(managerRequest.Data, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to delete genre with ID {GenreId}. Requested by {UserId}. Reason: {ErrorMessage}",
                managerRequest.Data, managerRequest.UserId, result.ErrorMessage);

            return new ManagerResponse<bool>
            {
                Success = false,
                Data = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        _logger.LogInformation("Successfully deleted genre with ID {GenreId}. Requested by {UserId}",
            managerRequest.Data, managerRequest.UserId);

        return new ManagerResponse<bool>
        {
            Success = true,
            Data = true,
            ResponseTime = DateTime.UtcNow,
        };
    }

    public async Task<ManagerResponse<PagedResult<GenreSummary>>> ListAsync(ManagerRequest<ListGenresRequest> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ListAsync called by UserId: {UserId}, RequestTime: {RequestTime}",
            managerRequest.UserId, managerRequest.RequestTime);

        var dalRequest = _mapper.Map<DAL.Genre.ListGenresRequest>(managerRequest.Data);
        var result = await _genreRepositoryAccess.ListAsync(dalRequest, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("ListAsync failed: {ErrorMessage}", result.ErrorMessage);
            return new ManagerResponse<PagedResult<GenreSummary>>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        _logger.LogInformation("ListAsync succeeded, TotalCount: {TotalCount}", result.TotalCount);

        return new ManagerResponse<PagedResult<GenreSummary>>
        {
            Success = true,
            Data = new PagedResult<GenreSummary>
            {
                Items = _mapper.Map<List<GenreSummary>>(result.Genres),
                TotalCount = result.TotalCount,
                Page = result.PageNumber,
                PageSize = result.PageSize,
            },
            ResponseTime = DateTime.UtcNow,
        };
    }
}