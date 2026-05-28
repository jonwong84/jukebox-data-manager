using AutoMapper;
using Jukebox.DataAccess.Interfaces;
using Jukebox.DataManager.Contracts;
using Jukebox.DataManager.Contracts.DataContracts.Album;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Microsoft.Extensions.Logging;
using DAL = Jukebox.DataAccess.Contracts.DataContracts;
using BLL = Jukebox.DataManager.Contracts.DataContracts;

namespace Jukebox.DataManager.Managers;

public sealed class AlbumManager : IAlbumManager
{
    private readonly IAlbumRepositoryAccess _albumRepositoryAccess;
    private readonly IMapper _mapper;
    private readonly ILogger<AlbumManager> _logger;

    public AlbumManager(IAlbumRepositoryAccess albumRepositoryAccess, IMapper mapper, ILogger<AlbumManager> logger)
    {
        _albumRepositoryAccess = albumRepositoryAccess;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ManagerResponse<AlbumDetails>> FindByIdAsync(ManagerRequest<int> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} requested album with ID {AlbumId} at {RequestTime}",
            managerRequest.UserId, managerRequest.Data, managerRequest.RequestTime);

        var result = await _albumRepositoryAccess.GetByIdAsync(managerRequest.Data, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Album with ID {AlbumId} was not found. Requested by {UserId}",
                managerRequest.Data, managerRequest.UserId);

            return new ManagerResponse<AlbumDetails>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        return new ManagerResponse<AlbumDetails>
        {
            Success = true,
            Data = _mapper.Map<AlbumDetails>(result.AlbumDetails),
            ResponseTime = DateTime.UtcNow,
        };
    }

    public async Task<ManagerResponse<AlbumSummary>> AddAlbumAsync(ManagerRequest<AddAlbumRequest> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} requested to add album with title {Title} at {RequestTime}",
            managerRequest.UserId, managerRequest.Data.Title, managerRequest.RequestTime);

        managerRequest.Data.UserId = managerRequest.UserId;
        var accessRequest = _mapper.Map<DAL.Album.AddAlbumRequest>(managerRequest.Data);
        var result = await _albumRepositoryAccess.AddAsync(accessRequest, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to add album with title {Title}. Requested by {UserId}. Reason: {ErrorMessage}",
                managerRequest.Data.Title, managerRequest.UserId, result.ErrorMessage);

            return new ManagerResponse<AlbumSummary>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        _logger.LogInformation("Successfully added album with ID {AlbumId}. Requested by {UserId}",
            result.AlbumId, managerRequest.UserId);

        return new ManagerResponse<AlbumSummary>
        {
            Success = true,
            Data = new AlbumSummary { Id = result.AlbumId!.Value, Title = managerRequest.Data.Title },
            ResponseTime = DateTime.UtcNow,
        };
    }

    public async Task<ManagerResponse<AlbumDetails>> UpdateAlbumAsync(ManagerRequest<UpdateAlbumRequest> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} requested to update album with ID {AlbumId} at {RequestTime}",
            managerRequest.UserId, managerRequest.Data.Id, managerRequest.RequestTime);

        managerRequest.Data.UserId = managerRequest.UserId;
        var accessRequest = _mapper.Map<DAL.Album.UpdateAlbumRequest>(managerRequest.Data);
        var result = await _albumRepositoryAccess.UpdateAsync(accessRequest, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to update album with ID {AlbumId}. Requested by {UserId}. Reason: {ErrorMessage}",
                managerRequest.Data.Id, managerRequest.UserId, result.ErrorMessage);

            return new ManagerResponse<AlbumDetails>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        _logger.LogInformation("Successfully updated album with ID {AlbumId}. Requested by {UserId}",
            managerRequest.Data.Id, managerRequest.UserId);

        return new ManagerResponse<AlbumDetails>
        {
            Success = true,
            Data = _mapper.Map<AlbumDetails>(result.AlbumDetails),
            ResponseTime = DateTime.UtcNow,
        };
    }

    public async Task<ManagerResponse<bool>> DeleteAlbumAsync(ManagerRequest<int> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} requested to delete album with ID {AlbumId} at {RequestTime}",
            managerRequest.UserId, managerRequest.Data, managerRequest.RequestTime);

        var result = await _albumRepositoryAccess.DeleteAsync(managerRequest.Data, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to delete album with ID {AlbumId}. Requested by {UserId}. Reason: {ErrorMessage}",
                managerRequest.Data, managerRequest.UserId, result.ErrorMessage);

            return new ManagerResponse<bool>
            {
                Success = false,
                Data = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        _logger.LogInformation("Successfully deleted album with ID {AlbumId}. Requested by {UserId}",
            managerRequest.Data, managerRequest.UserId);

        return new ManagerResponse<bool>
        {
            Success = true,
            Data = true,
            ResponseTime = DateTime.UtcNow,
        };
    }
}