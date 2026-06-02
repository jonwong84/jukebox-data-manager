using AutoMapper;
using Jukebox.DataAccess.Contracts.DataContracts.Artist;
using Jukebox.DataAccess.Interfaces;
using Jukebox.DataManager.Contracts.DataContracts.Artist;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Microsoft.Extensions.Logging;
using Moq;
using DAL = Jukebox.DataAccess.Contracts.DataContracts;
using BLL = Jukebox.DataManager.Contracts.DataContracts;

namespace Jukebox.DataManager.Managers.Test.UnitTests;

public class ArtistManagerTests
{
    private readonly Mock<IArtistRepositoryAccess> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ArtistManager>> _loggerMock;
    private readonly ArtistManager _sut;

    public ArtistManagerTests()
    {
        _repositoryMock = new Mock<IArtistRepositoryAccess>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ArtistManager>>();
        _sut = new ArtistManager(_repositoryMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    // -------------------------------------------------------------------------
    // FindByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FindByIdAsync_WhenRepositoryReturnsSuccess_ReturnsMappedArtistDetails()
    {
        // Arrange
        var dalDetails = new DAL.Artist.ArtistDetails { Id = 1, Name = "Test Artist" };
        var bllDetails = new BLL.Artist.ArtistDetails { Id = 1, Name = "Test Artist" };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetArtistResult { Success = true, ArtistDetails = dalDetails });

        _mapperMock
            .Setup(m => m.Map<BLL.Artist.ArtistDetails>(dalDetails))
            .Returns(bllDetails);

        var request = new ManagerRequest<int> { Data = 1, UserId = "user-1" };

        // Act
        var response = await _sut.FindByIdAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(bllDetails, response.Data);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task FindByIdAsync_WhenRepositoryReturnsFailure_ReturnsFailureResponse()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetArtistResult { Success = false, ErrorMessage = "Artist not found" });

        var request = new ManagerRequest<int> { Data = 99, UserId = "user-1" };

        // Act
        var response = await _sut.FindByIdAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Artist not found", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // AddArtistAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddArtistAsync_WhenRepositoryReturnsSuccess_ReturnsArtistSummary()
    {
        // Arrange
        var addRequest = new BLL.Artist.AddArtistRequest { Name = "New Artist", Bio = "Some bio" };
        var dalAddRequest = new DAL.Artist.AddArtistRequest();

        _mapperMock
            .Setup(m => m.Map<DAL.Artist.AddArtistRequest>(addRequest))
            .Returns(dalAddRequest);

        _repositoryMock
            .Setup(r => r.AddAsync(dalAddRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AddArtistResult { Success = true, ArtistId = 42 });

        var request = new ManagerRequest<BLL.Artist.AddArtistRequest> { Data = addRequest, UserId = "user-1" };

        // Act
        var response = await _sut.AddArtistAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(42, response.Data.Id);
        Assert.Equal("New Artist", response.Data.Name);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task AddArtistAsync_WhenRepositoryReturnsFailure_ReturnsFailureResponse()
    {
        // Arrange
        var addRequest = new BLL.Artist.AddArtistRequest { Name = "New Artist" };
        var dalAddRequest = new DAL.Artist.AddArtistRequest();

        _mapperMock
            .Setup(m => m.Map<DAL.Artist.AddArtistRequest>(addRequest))
            .Returns(dalAddRequest);

        _repositoryMock
            .Setup(r => r.AddAsync(dalAddRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AddArtistResult { Success = false, ErrorMessage = "Duplicate artist name" });

        var request = new ManagerRequest<BLL.Artist.AddArtistRequest> { Data = addRequest, UserId = "user-1" };

        // Act
        var response = await _sut.AddArtistAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Duplicate artist name", response.ErrorMessage);
    }

    [Fact]
    public async Task AddArtistAsync_SetsUserIdOnRequestData_BeforeMappingAndCalling()
    {
        // Arrange
        var addRequest = new BLL.Artist.AddArtistRequest { Name = "New Artist" };
        var dalAddRequest = new DAL.Artist.AddArtistRequest();

        _mapperMock
            .Setup(m => m.Map<DAL.Artist.AddArtistRequest>(addRequest))
            .Returns(dalAddRequest);

        _repositoryMock
            .Setup(r => r.AddAsync(dalAddRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AddArtistResult { Success = true, ArtistId = 1 });

        var request = new ManagerRequest<BLL.Artist.AddArtistRequest> { Data = addRequest, UserId = "user-99" };

        // Act
        await _sut.AddArtistAsync(request);

        // Assert
        Assert.Equal("user-99", addRequest.UserId);
    }

    // -------------------------------------------------------------------------
    // UpdateArtistAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateArtistAsync_WhenRepositoryReturnsSuccess_ReturnsMappedArtistDetails()
    {
        // Arrange
        var updateRequest = new BLL.Artist.UpdateArtistRequest { Id = 1, Name = "Updated Artist" };
        var dalUpdateRequest = new DAL.Artist.UpdateArtistRequest();
        var dalDetails = new DAL.Artist.ArtistDetails { Id = 1, Name = "Updated Artist" };
        var bllDetails = new BLL.Artist.ArtistDetails { Id = 1, Name = "Updated Artist" };

        _mapperMock
            .Setup(m => m.Map<DAL.Artist.UpdateArtistRequest>(updateRequest))
            .Returns(dalUpdateRequest);

        _repositoryMock
            .Setup(r => r.UpdateAsync(dalUpdateRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateArtistResult { Success = true, ArtistDetails = dalDetails });

        _mapperMock
            .Setup(m => m.Map<BLL.Artist.ArtistDetails>(dalDetails))
            .Returns(bllDetails);

        var request = new ManagerRequest<BLL.Artist.UpdateArtistRequest> { Data = updateRequest, UserId = "user-1" };

        // Act
        var response = await _sut.UpdateArtistAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(bllDetails, response.Data);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task UpdateArtistAsync_WhenRepositoryReturnsFailure_ReturnsFailureResponse()
    {
        // Arrange
        var updateRequest = new BLL.Artist.UpdateArtistRequest { Id = 99, Name = "Updated Artist" };
        var dalUpdateRequest = new DAL.Artist.UpdateArtistRequest();

        _mapperMock
            .Setup(m => m.Map<DAL.Artist.UpdateArtistRequest>(updateRequest))
            .Returns(dalUpdateRequest);

        _repositoryMock
            .Setup(r => r.UpdateAsync(dalUpdateRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateArtistResult { Success = false, ErrorMessage = "Artist not found" });

        var request = new ManagerRequest<BLL.Artist.UpdateArtistRequest> { Data = updateRequest, UserId = "user-1" };

        // Act
        var response = await _sut.UpdateArtistAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Artist not found", response.ErrorMessage);
    }

    [Fact]
    public async Task UpdateArtistAsync_SetsUserIdOnRequestData_BeforeMappingAndCalling()
    {
        // Arrange
        var updateRequest = new BLL.Artist.UpdateArtistRequest { Id = 1, Name = "Updated Artist" };
        var dalUpdateRequest = new DAL.Artist.UpdateArtistRequest();
        var dalDetails = new DAL.Artist.ArtistDetails { Id = 1, Name = "Updated Artist" };
        var bllDetails = new BLL.Artist.ArtistDetails { Id = 1, Name = "Updated Artist" };

        _mapperMock
            .Setup(m => m.Map<DAL.Artist.UpdateArtistRequest>(updateRequest))
            .Returns(dalUpdateRequest);

        _repositoryMock
            .Setup(r => r.UpdateAsync(dalUpdateRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateArtistResult { Success = true, ArtistDetails = dalDetails });

        _mapperMock
            .Setup(m => m.Map<BLL.Artist.ArtistDetails>(dalDetails))
            .Returns(bllDetails);

        var request = new ManagerRequest<BLL.Artist.UpdateArtistRequest> { Data = updateRequest, UserId = "user-99" };

        // Act
        await _sut.UpdateArtistAsync(request);

        // Assert
        Assert.Equal("user-99", updateRequest.UserId);
    }

    // -------------------------------------------------------------------------
    // DeleteArtistAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteArtistAsync_WhenRepositoryReturnsSuccess_ReturnsTrue()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteArtistResult { Success = true });

        var request = new ManagerRequest<int> { Data = 1, UserId = "user-1" };

        // Act
        var response = await _sut.DeleteArtistAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.True(response.Data);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task DeleteArtistAsync_WhenRepositoryReturnsFailure_ReturnsFalseWithErrorMessage()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteArtistResult { Success = false, ErrorMessage = "Artist not found" });

        var request = new ManagerRequest<int> { Data = 99, UserId = "user-1" };

        // Act
        var response = await _sut.DeleteArtistAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.False(response.Data);
        Assert.Equal("Artist not found", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // ListAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListAsync_WhenRepositoryReturnsSuccess_ReturnsMappedPagedResult()
    {
        // Arrange
        var listRequest = new BLL.Artist.ListArtistsRequest { PageNumber = 1, PageSize = 10 };
        var dalListRequest = new DAL.Artist.ListArtistsRequest();
        var dalSummaries = new List<DAL.Artist.ArtistSummary>
        {
            new() { Id = 1, Name = "Artist One" },
            new() { Id = 2, Name = "Artist Two" }
        };
        var bllSummaries = new List<BLL.Artist.ArtistSummary>
        {
            new() { Id = 1, Name = "Artist One" },
            new() { Id = 2, Name = "Artist Two" }
        };

        _mapperMock
            .Setup(m => m.Map<DAL.Artist.ListArtistsRequest>(listRequest))
            .Returns(dalListRequest);

        _repositoryMock
            .Setup(r => r.ListAsync(dalListRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListArtistsResult
            {
                Success = true,
                Artists = dalSummaries,
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            });

        _mapperMock
            .Setup(m => m.Map<List<BLL.Artist.ArtistSummary>>(dalSummaries))
            .Returns(bllSummaries);

        var request = new ManagerRequest<BLL.Artist.ListArtistsRequest> { Data = listRequest, UserId = "user-1" };

        // Act
        var response = await _sut.ListAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Items.Count);
        Assert.Equal(2, response.Data.TotalCount);
        Assert.Equal(1, response.Data.Page);
        Assert.Equal(10, response.Data.PageSize);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task ListAsync_WhenRepositoryReturnsFailure_ReturnsFailureResponse()
    {
        // Arrange
        var listRequest = new BLL.Artist.ListArtistsRequest { PageNumber = 1, PageSize = 10 };
        var dalListRequest = new DAL.Artist.ListArtistsRequest();

        _mapperMock
            .Setup(m => m.Map<DAL.Artist.ListArtistsRequest>(listRequest))
            .Returns(dalListRequest);

        _repositoryMock
            .Setup(r => r.ListAsync(dalListRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListArtistsResult { Success = false, ErrorMessage = "Database error" });

        var request = new ManagerRequest<BLL.Artist.ListArtistsRequest> { Data = listRequest, UserId = "user-1" };

        // Act
        var response = await _sut.ListAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Database error", response.ErrorMessage);
    }
}
