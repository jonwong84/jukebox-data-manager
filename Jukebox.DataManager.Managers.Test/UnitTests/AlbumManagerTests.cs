using AutoMapper;
using Jukebox.DataAccess.Contracts.DataContracts.Album;
using Jukebox.DataAccess.Interfaces;
using Jukebox.DataManager.Contracts.DataContracts.Album;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Microsoft.Extensions.Logging;
using Moq;
using DAL = Jukebox.DataAccess.Contracts.DataContracts;
using BLL = Jukebox.DataManager.Contracts.DataContracts;

namespace Jukebox.DataManager.Managers.Test.UnitTests;

public class AlbumManagerTests
{
    private readonly Mock<IAlbumRepositoryAccess> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AlbumManager>> _loggerMock;
    private readonly AlbumManager _sut;

    public AlbumManagerTests()
    {
        _repositoryMock = new Mock<IAlbumRepositoryAccess>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AlbumManager>>();
        _sut = new AlbumManager(_repositoryMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    // -------------------------------------------------------------------------
    // FindByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FindByIdAsync_WhenRepositoryReturnsSuccess_ReturnsMappedAlbumDetails()
    {
        // Arrange
        var dalDetails = new DAL.Album.AlbumDetails { Id = 1, Title = "Test Album" };
        var bllDetails = new BLL.Album.AlbumDetails { Id = 1, Title = "Test Album" };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetAlbumResult { Success = true, AlbumDetails = dalDetails });

        _mapperMock
            .Setup(m => m.Map<BLL.Album.AlbumDetails>(dalDetails))
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
            .ReturnsAsync(new GetAlbumResult { Success = false, ErrorMessage = "Album not found" });

        var request = new ManagerRequest<int> { Data = 99, UserId = "user-1" };

        // Act
        var response = await _sut.FindByIdAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Album not found", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // AddAlbumAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAlbumAsync_WhenRepositoryReturnsSuccess_ReturnsAlbumSummary()
    {
        // Arrange
        var addRequest = new BLL.Album.AddAlbumRequest { Title = "New Album", ArtistIds = [1, 2] };
        var dalAddRequest = new DAL.Album.AddAlbumRequest();

        _mapperMock
            .Setup(m => m.Map<DAL.Album.AddAlbumRequest>(addRequest))
            .Returns(dalAddRequest);

        _repositoryMock
            .Setup(r => r.AddAsync(dalAddRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AddAlbumResult { Success = true, AlbumId = 42 });

        var request = new ManagerRequest<BLL.Album.AddAlbumRequest> { Data = addRequest, UserId = "user-1" };

        // Act
        var response = await _sut.AddAlbumAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(42, response.Data.Id);
        Assert.Equal("New Album", response.Data.Title);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task AddAlbumAsync_WhenRepositoryReturnsFailure_ReturnsFailureResponse()
    {
        // Arrange
        var addRequest = new BLL.Album.AddAlbumRequest { Title = "New Album" };
        var dalAddRequest = new DAL.Album.AddAlbumRequest();

        _mapperMock
            .Setup(m => m.Map<DAL.Album.AddAlbumRequest>(addRequest))
            .Returns(dalAddRequest);

        _repositoryMock
            .Setup(r => r.AddAsync(dalAddRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AddAlbumResult { Success = false, ErrorMessage = "Duplicate album title" });

        var request = new ManagerRequest<BLL.Album.AddAlbumRequest> { Data = addRequest, UserId = "user-1" };

        // Act
        var response = await _sut.AddAlbumAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Duplicate album title", response.ErrorMessage);
    }

    [Fact]
    public async Task AddAlbumAsync_SetsUserIdOnRequestData_BeforeMappingAndCalling()
    {
        // Arrange
        var addRequest = new BLL.Album.AddAlbumRequest { Title = "New Album" };
        var dalAddRequest = new DAL.Album.AddAlbumRequest();

        _mapperMock
            .Setup(m => m.Map<DAL.Album.AddAlbumRequest>(addRequest))
            .Returns(dalAddRequest);

        _repositoryMock
            .Setup(r => r.AddAsync(dalAddRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AddAlbumResult { Success = true, AlbumId = 1 });

        var request = new ManagerRequest<BLL.Album.AddAlbumRequest> { Data = addRequest, UserId = "user-99" };

        // Act
        await _sut.AddAlbumAsync(request);

        // Assert
        Assert.Equal("user-99", addRequest.UserId);
    }

    // -------------------------------------------------------------------------
    // UpdateAlbumAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAlbumAsync_WhenRepositoryReturnsSuccess_ReturnsMappedAlbumDetails()
    {
        // Arrange
        var updateRequest = new BLL.Album.UpdateAlbumRequest { Id = 1, Title = "Updated Album" };
        var dalUpdateRequest = new DAL.Album.UpdateAlbumRequest();
        var dalDetails = new DAL.Album.AlbumDetails { Id = 1, Title = "Updated Album" };
        var bllDetails = new BLL.Album.AlbumDetails { Id = 1, Title = "Updated Album" };

        _mapperMock
            .Setup(m => m.Map<DAL.Album.UpdateAlbumRequest>(updateRequest))
            .Returns(dalUpdateRequest);

        _repositoryMock
            .Setup(r => r.UpdateAsync(dalUpdateRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateAlbumResult { Success = true, AlbumDetails = dalDetails });

        _mapperMock
            .Setup(m => m.Map<BLL.Album.AlbumDetails>(dalDetails))
            .Returns(bllDetails);

        var request = new ManagerRequest<BLL.Album.UpdateAlbumRequest> { Data = updateRequest, UserId = "user-1" };

        // Act
        var response = await _sut.UpdateAlbumAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(bllDetails, response.Data);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAlbumAsync_WhenRepositoryReturnsFailure_ReturnsFailureResponse()
    {
        // Arrange
        var updateRequest = new BLL.Album.UpdateAlbumRequest { Id = 99, Title = "Updated Album" };
        var dalUpdateRequest = new DAL.Album.UpdateAlbumRequest();

        _mapperMock
            .Setup(m => m.Map<DAL.Album.UpdateAlbumRequest>(updateRequest))
            .Returns(dalUpdateRequest);

        _repositoryMock
            .Setup(r => r.UpdateAsync(dalUpdateRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateAlbumResult { Success = false, ErrorMessage = "Album not found" });

        var request = new ManagerRequest<BLL.Album.UpdateAlbumRequest> { Data = updateRequest, UserId = "user-1" };

        // Act
        var response = await _sut.UpdateAlbumAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Album not found", response.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAlbumAsync_SetsUserIdOnRequestData_BeforeMappingAndCalling()
    {
        // Arrange
        var updateRequest = new BLL.Album.UpdateAlbumRequest { Id = 1, Title = "Updated Album" };
        var dalUpdateRequest = new DAL.Album.UpdateAlbumRequest();
        var dalDetails = new DAL.Album.AlbumDetails { Id = 1, Title = "Updated Album" };
        var bllDetails = new BLL.Album.AlbumDetails { Id = 1, Title = "Updated Album" };

        _mapperMock
            .Setup(m => m.Map<DAL.Album.UpdateAlbumRequest>(updateRequest))
            .Returns(dalUpdateRequest);

        _repositoryMock
            .Setup(r => r.UpdateAsync(dalUpdateRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateAlbumResult { Success = true, AlbumDetails = dalDetails });

        _mapperMock
            .Setup(m => m.Map<BLL.Album.AlbumDetails>(dalDetails))
            .Returns(bllDetails);

        var request = new ManagerRequest<BLL.Album.UpdateAlbumRequest> { Data = updateRequest, UserId = "user-99" };

        // Act
        await _sut.UpdateAlbumAsync(request);

        // Assert
        Assert.Equal("user-99", updateRequest.UserId);
    }

    // -------------------------------------------------------------------------
    // DeleteAlbumAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAlbumAsync_WhenRepositoryReturnsSuccess_ReturnsTrue()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteAlbumResult { Success = true });

        var request = new ManagerRequest<int> { Data = 1, UserId = "user-1" };

        // Act
        var response = await _sut.DeleteAlbumAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.True(response.Data);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAlbumAsync_WhenRepositoryReturnsFailure_ReturnsFalseWithErrorMessage()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteAlbumResult { Success = false, ErrorMessage = "Album not found" });

        var request = new ManagerRequest<int> { Data = 99, UserId = "user-1" };

        // Act
        var response = await _sut.DeleteAlbumAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.False(response.Data);
        Assert.Equal("Album not found", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // ListAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListAsync_WhenRepositoryReturnsSuccess_ReturnsMappedPagedResult()
    {
        // Arrange
        var listRequest = new BLL.Album.ListAlbumsRequest { PageNumber = 1, PageSize = 10 };
        var dalListRequest = new DAL.Album.ListAlbumsRequest();
        var dalSummaries = new List<DAL.Album.AlbumSummary>
        {
            new() { Id = 1, Title = "Album One" },
            new() { Id = 2, Title = "Album Two" }
        };
        var bllSummaries = new List<BLL.Album.AlbumSummary>
        {
            new() { Id = 1, Title = "Album One" },
            new() { Id = 2, Title = "Album Two" }
        };

        _mapperMock
            .Setup(m => m.Map<DAL.Album.ListAlbumsRequest>(listRequest))
            .Returns(dalListRequest);

        _repositoryMock
            .Setup(r => r.ListAsync(dalListRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListAlbumsResult
            {
                Success = true,
                Albums = dalSummaries,
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            });

        _mapperMock
            .Setup(m => m.Map<List<BLL.Album.AlbumSummary>>(dalSummaries))
            .Returns(bllSummaries);

        var request = new ManagerRequest<BLL.Album.ListAlbumsRequest> { Data = listRequest, UserId = "user-1" };

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
        var listRequest = new BLL.Album.ListAlbumsRequest { PageNumber = 1, PageSize = 10 };
        var dalListRequest = new DAL.Album.ListAlbumsRequest();

        _mapperMock
            .Setup(m => m.Map<DAL.Album.ListAlbumsRequest>(listRequest))
            .Returns(dalListRequest);

        _repositoryMock
            .Setup(r => r.ListAsync(dalListRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListAlbumsResult { Success = false, ErrorMessage = "Database error" });

        var request = new ManagerRequest<BLL.Album.ListAlbumsRequest> { Data = listRequest, UserId = "user-1" };

        // Act
        var response = await _sut.ListAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Database error", response.ErrorMessage);
    }
}