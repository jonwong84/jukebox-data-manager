using AutoMapper;
using Jukebox.DataAccess.Interfaces;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Song;
using Jukebox.DataManager.Managers.Interfaces;
using Microsoft.Extensions.Logging;
using DAL = Jukebox.DataAccess.Contracts.DataContracts;

namespace Jukebox.DataManager.Managers;

public sealed class SongManager : ISongManager
{
    private readonly ISongRepositoryAccess _songRepositoryAccess;
    private readonly IMapper _mapper;
    private readonly ILogger<SongManager> _logger;

    public SongManager(ISongRepositoryAccess songRepositoryAccess, IMapper mapper, ILogger<SongManager> logger)
    {
        _songRepositoryAccess = songRepositoryAccess;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ManagerResponse<SongSummary>> AddSongAsync(ManagerRequest<AddSongRequest> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} requested to add song with title {Title} at {RequestTime}",
            managerRequest.UserId, managerRequest.Data.Title, managerRequest.RequestTime);

        managerRequest.Data.UserId = managerRequest.UserId;
        var accessRequest = _mapper.Map<DAL.Song.AddSongRequest>(managerRequest.Data);
        var result = await _songRepositoryAccess.AddAsync(accessRequest, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to add song with title {Title}. Requested by {UserId}. Reason: {ErrorMessage}",
                managerRequest.Data.Title, managerRequest.UserId, result.ErrorMessage);

            return new ManagerResponse<SongSummary>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        _logger.LogInformation("Successfully added song with ID {SongId}. Requested by {UserId}",
            result.SongId, managerRequest.UserId);

        return new ManagerResponse<SongSummary>
        {
            Success = true,
            Data = new SongSummary { Id = result.SongId!.Value },
            ResponseTime = DateTime.UtcNow,
        };
    }

    public async Task<ManagerResponse<bool>> DeleteSongAsync(ManagerRequest<int> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} requested to delete song with ID {SongId} at {RequestTime}",
            managerRequest.UserId, managerRequest.Data, managerRequest.RequestTime);

        var result = await _songRepositoryAccess.DeleteAsync(managerRequest.Data, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to delete song with ID {SongId}. Requested by {UserId}. Reason: {ErrorMessage}",
                managerRequest.Data, managerRequest.UserId, result.ErrorMessage);

            return new ManagerResponse<bool>
            {
                Success = false,
                Data = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        _logger.LogInformation("Successfully deleted song with ID {SongId}. Requested by {UserId}",
            managerRequest.Data, managerRequest.UserId);

        return new ManagerResponse<bool>
        {
            Success = true,
            Data = true,
            ResponseTime = DateTime.UtcNow,
        };
    }

    public async Task<ManagerResponse<SongDetails>> FindByIdAsync(ManagerRequest<int> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} requested song with ID {SongId} at {RequestTime}",
            managerRequest.UserId, managerRequest.Data, managerRequest.RequestTime);

        var result = await _songRepositoryAccess.GetByIdAsync(managerRequest.Data, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Song with ID {SongId} was not found. Requested by {UserId}",
                managerRequest.Data, managerRequest.UserId);

            return new ManagerResponse<SongDetails>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        return new ManagerResponse<SongDetails>
        {
            Success = true,
            Data = _mapper.Map<SongDetails>(result.SongDetails),
            ResponseTime = DateTime.UtcNow,
        };
    }

    public async Task<ManagerResponse<SongDetails>> UpdateSongAsync(ManagerRequest<UpdateSongRequest> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} requested to update song with ID {SongId} at {RequestTime}",
            managerRequest.UserId, managerRequest.Data.Id, managerRequest.RequestTime);

        managerRequest.Data.UserId = managerRequest.UserId;

        var accessRequest = _mapper.Map<DAL.Song.UpdateSongRequest>(managerRequest.Data);
        var result = await _songRepositoryAccess.UpdateAsync(accessRequest, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to update song with ID {SongId}. Requested by {UserId}. Reason: {ErrorMessage}",
                managerRequest.Data.Id, managerRequest.UserId, result.ErrorMessage);

            return new ManagerResponse<SongDetails>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        _logger.LogInformation("Successfully updated song with ID {SongId}. Requested by {UserId}",
            managerRequest.Data.Id, managerRequest.UserId);

        return new ManagerResponse<SongDetails>
        {
            Success = true,
            Data = _mapper.Map<SongDetails>(result.SongDetails),
            ResponseTime = DateTime.UtcNow,
        };
    }

    public async Task<ManagerResponse<PagedResult<SongSummary>>> ListAsync(ManagerRequest<ListSongsRequest> request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ListAsync called by UserId: {UserId}, RequestTime: {RequestTime}",
            request.UserId, request.RequestTime);

        var dalRequest = _mapper.Map<DAL.Song.ListSongsRequest>(request.Data);
        dalRequest.UserId = request.UserId;

        var result = await _songRepositoryAccess.ListAsync(dalRequest, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("ListAsync failed: {ErrorMessage}", result.ErrorMessage);
            return new ManagerResponse<PagedResult<SongSummary>>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow
            };
        }

        _logger.LogInformation("ListAsync succeeded, TotalCount: {TotalCount}", result.TotalCount);

        return new ManagerResponse<PagedResult<SongSummary>>
        {
            Success = true,
            Data = new PagedResult<SongSummary>
            {
                Items = _mapper.Map<List<SongSummary>>(result.Songs),
                TotalCount = result.TotalCount,
                Page = result.PageNumber,
                PageSize = result.PageSize
            },
            ResponseTime = DateTime.UtcNow
        };
    }
}
