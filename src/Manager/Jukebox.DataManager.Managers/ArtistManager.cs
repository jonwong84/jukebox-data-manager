using AutoMapper;
using Jukebox.DataAccess.Interfaces;
using Jukebox.DataManager.Contracts;
using Jukebox.DataManager.Contracts.DataContracts.Artist;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Microsoft.Extensions.Logging;
using DAL = Jukebox.DataAccess.Contracts.DataContracts;
using BLL = Jukebox.DataManager.Contracts.DataContracts;

namespace Jukebox.DataManager.Managers;

public sealed class ArtistManager : IArtistManager
{
    private readonly IArtistRepositoryAccess _artistRepositoryAccess;
    private readonly IMapper _mapper;
    private readonly ILogger<ArtistManager> _logger;

    public ArtistManager(IArtistRepositoryAccess artistRepositoryAccess, IMapper mapper, ILogger<ArtistManager> logger)
    {
        _artistRepositoryAccess = artistRepositoryAccess;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ManagerResponse<ArtistDetails>> FindByIdAsync(ManagerRequest<int> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} requested artist with ID {ArtistId} at {RequestTime}",
            managerRequest.UserId, managerRequest.Data, managerRequest.RequestTime);

        var result = await _artistRepositoryAccess.GetByIdAsync(managerRequest.Data, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Artist with ID {ArtistId} was not found. Requested by {UserId}",
                managerRequest.Data, managerRequest.UserId);

            return new ManagerResponse<ArtistDetails>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        return new ManagerResponse<ArtistDetails>
        {
            Success = true,
            Data = _mapper.Map<ArtistDetails>(result.ArtistDetails),
            ResponseTime = DateTime.UtcNow,
        };
    }

    public async Task<ManagerResponse<ArtistSummary>> AddArtistAsync(ManagerRequest<AddArtistRequest> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} requested to add artist with name {Name} at {RequestTime}",
            managerRequest.UserId, managerRequest.Data.Name, managerRequest.RequestTime);

        managerRequest.Data.UserId = managerRequest.UserId;
        var accessRequest = _mapper.Map<DAL.Artist.AddArtistRequest>(managerRequest.Data);
        var result = await _artistRepositoryAccess.AddAsync(accessRequest, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to add artist with name {Name}. Requested by {UserId}. Reason: {ErrorMessage}",
                managerRequest.Data.Name, managerRequest.UserId, result.ErrorMessage);

            return new ManagerResponse<ArtistSummary>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        _logger.LogInformation("Successfully added artist with ID {ArtistId}. Requested by {UserId}",
            result.ArtistId, managerRequest.UserId);

        return new ManagerResponse<ArtistSummary>
        {
            Success = true,
            Data = new ArtistSummary { Id = result.ArtistId!.Value, Name = managerRequest.Data.Name },
            ResponseTime = DateTime.UtcNow,
        };
    }

    public async Task<ManagerResponse<ArtistDetails>> UpdateArtistAsync(ManagerRequest<UpdateArtistRequest> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} requested to update artist with ID {ArtistId} at {RequestTime}",
            managerRequest.UserId, managerRequest.Data.Id, managerRequest.RequestTime);

        managerRequest.Data.UserId = managerRequest.UserId;
        var accessRequest = _mapper.Map<DAL.Artist.UpdateArtistRequest>(managerRequest.Data);
        var result = await _artistRepositoryAccess.UpdateAsync(accessRequest, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to update artist with ID {ArtistId}. Requested by {UserId}. Reason: {ErrorMessage}",
                managerRequest.Data.Id, managerRequest.UserId, result.ErrorMessage);

            return new ManagerResponse<ArtistDetails>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        _logger.LogInformation("Successfully updated artist with ID {ArtistId}. Requested by {UserId}",
            managerRequest.Data.Id, managerRequest.UserId);

        return new ManagerResponse<ArtistDetails>
        {
            Success = true,
            Data = _mapper.Map<ArtistDetails>(result.ArtistDetails),
            ResponseTime = DateTime.UtcNow,
        };
    }

    public async Task<ManagerResponse<bool>> DeleteArtistAsync(ManagerRequest<int> managerRequest, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} requested to delete artist with ID {ArtistId} at {RequestTime}",
            managerRequest.UserId, managerRequest.Data, managerRequest.RequestTime);

        var result = await _artistRepositoryAccess.DeleteAsync(managerRequest.Data, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Failed to delete artist with ID {ArtistId}. Requested by {UserId}. Reason: {ErrorMessage}",
                managerRequest.Data, managerRequest.UserId, result.ErrorMessage);

            return new ManagerResponse<bool>
            {
                Success = false,
                Data = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow,
            };
        }

        _logger.LogInformation("Successfully deleted artist with ID {ArtistId}. Requested by {UserId}",
            managerRequest.Data, managerRequest.UserId);

        return new ManagerResponse<bool>
        {
            Success = true,
            Data = true,
            ResponseTime = DateTime.UtcNow,
        };
    }

    public async Task<ManagerResponse<PagedResult<ArtistSummary>>> ListAsync(ManagerRequest<ListArtistsRequest> request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ListAsync called by UserId: {UserId}, RequestTime: {RequestTime}",
            request.UserId, request.RequestTime);

        var dalRequest = _mapper.Map<DAL.Artist.ListArtistsRequest>(request.Data);
        dalRequest.UserId = request.UserId;

        var result = await _artistRepositoryAccess.ListAsync(dalRequest, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("ListAsync failed: {ErrorMessage}", result.ErrorMessage);
            return new ManagerResponse<PagedResult<ArtistSummary>>
            {
                Success = false,
                ErrorMessage = result.ErrorMessage,
                ResponseTime = DateTime.UtcNow
            };
        }

        _logger.LogInformation("ListAsync succeeded, TotalCount: {TotalCount}", result.TotalCount);

        return new ManagerResponse<PagedResult<ArtistSummary>>
        {
            Success = true,
            Data = new PagedResult<ArtistSummary>
            {
                Items = _mapper.Map<List<ArtistSummary>>(result.Artists),
                TotalCount = result.TotalCount,
                Page = result.PageNumber,
                PageSize = result.PageSize
            },
            ResponseTime = DateTime.UtcNow
        };
    }
}