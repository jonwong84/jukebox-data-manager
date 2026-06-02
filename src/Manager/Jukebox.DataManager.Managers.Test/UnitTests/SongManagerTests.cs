using AutoMapper;
using Jukebox.DataAccess.Contracts.DataContracts.Song;
using Jukebox.DataAccess.Interfaces;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Song;
using Microsoft.Extensions.Logging;
using Moq;
using DAL = Jukebox.DataAccess.Contracts.DataContracts;
using BLL = Jukebox.DataManager.Contracts.DataContracts;

namespace Jukebox.DataManager.Managers.Test.UnitTests;

public class SongManagerTests
{
    private readonly Mock<ISongRepositoryAccess> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<SongManager>> _loggerMock;
    private readonly SongManager _sut;

    public SongManagerTests()
    {
        _repositoryMock = new Mock<ISongRepositoryAccess>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<SongManager>>();
        _sut = new SongManager(_repositoryMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    // -------------------------------------------------------------------------
    // FindByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FindByIdAsync_WhenRepositoryReturnsSuccess_ReturnsMappedSongDetails()
    {
        // Arrange
        var dalDetails = new DAL.Song.SongDetails { Id = 1, Title = "Test Song" };
        var bllDetails = new BLL.Song.SongDetails { Id = 1, Title = "Test Song" };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Song.GetSongResult { Success = true, SongDetails = dalDetails });

        _mapperMock
            .Setup(m => m.Map<BLL.Song.SongDetails>(dalDetails))
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
            .ReturnsAsync(new DAL.Song.GetSongResult { Success = false, ErrorMessage = "Song not found" });

        var request = new ManagerRequest<int> { Data = 99, UserId = "user-1" };

        // Act
        var response = await _sut.FindByIdAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Song not found", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // AddSongAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddSongAsync_WhenRepositoryReturnsSuccess_ReturnsSongSummaryWithId()
    {
        // Arrange
        var addRequest = new BLL.Song.AddSongRequest { Title = "New Song", ArtistId = 1 };
        var dalAddRequest = new DAL.Song.AddSongRequest();

        _mapperMock
            .Setup(m => m.Map<DAL.Song.AddSongRequest>(addRequest))
            .Returns(dalAddRequest);

        _repositoryMock
            .Setup(r => r.AddAsync(dalAddRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Song.AddSongResult { Success = true, SongId = 42 });

        var request = new ManagerRequest<BLL.Song.AddSongRequest> { Data = addRequest, UserId = "user-1" };

        // Act
        var response = await _sut.AddSongAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(42, response.Data.Id);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task AddSongAsync_WhenRepositoryReturnsFailure_ReturnsFailureResponse()
    {
        // Arrange
        var addRequest = new BLL.Song.AddSongRequest { Title = "New Song", ArtistId = 1 };
        var dalAddRequest = new DAL.Song.AddSongRequest();

        _mapperMock
            .Setup(m => m.Map<DAL.Song.AddSongRequest>(addRequest))
            .Returns(dalAddRequest);

        _repositoryMock
            .Setup(r => r.AddAsync(dalAddRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Song.AddSongResult { Success = false, ErrorMessage = "Artist not found" });
        var request = new ManagerRequest<BLL.Song.AddSongRequest> { Data = addRequest, UserId = "user-1" };

        // Act
        var response = await _sut.AddSongAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Artist not found", response.ErrorMessage);
    }

    [Fact]
    public async Task AddSongAsync_SetsUserIdOnRequestData_BeforeMappingAndCalling()
    {
        // Arrange
        var addRequest = new BLL.Song.AddSongRequest { Title = "New Song", ArtistId = 1 };
        var dalAddRequest = new DAL.Song.AddSongRequest();

        _mapperMock
            .Setup(m => m.Map<DAL.Song.AddSongRequest>(addRequest))
            .Returns(dalAddRequest);

        _repositoryMock
            .Setup(r => r.AddAsync(dalAddRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Song.AddSongResult { Success = true, SongId = 1 });

        var request = new ManagerRequest<BLL.Song.AddSongRequest> { Data = addRequest, UserId = "user-99" };

        // Act
        await _sut.AddSongAsync(request);

        // Assert
        Assert.Equal("user-99", addRequest.UserId);
    }

    // -------------------------------------------------------------------------
    // UpdateSongAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateSongAsync_WhenRepositoryReturnsSuccess_ReturnsMappedSongDetails()
    {
        // Arrange
        var updateRequest = new BLL.Song.UpdateSongRequest { Id = 1, Title = "Updated Song", ArtistId = 1 };
        var dalUpdateRequest = new DAL.Song.UpdateSongRequest();
        var dalDetails = new DAL.Song.SongDetails { Id = 1, Title = "Updated Song" };
        var bllDetails = new BLL.Song.SongDetails { Id = 1, Title = "Updated Song" };

        _mapperMock
            .Setup(m => m.Map<DAL.Song.UpdateSongRequest>(updateRequest))
            .Returns(dalUpdateRequest);

        _repositoryMock
            .Setup(r => r.UpdateAsync(dalUpdateRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Song.UpdateSongResult { Success = true, SongDetails = dalDetails });

        _mapperMock
            .Setup(m => m.Map<BLL.Song.SongDetails>(dalDetails))
            .Returns(bllDetails);

        var request = new ManagerRequest<BLL.Song.UpdateSongRequest> { Data = updateRequest, UserId = "user-1" };

        // Act
        var response = await _sut.UpdateSongAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(bllDetails, response.Data);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task UpdateSongAsync_WhenRepositoryReturnsFailure_ReturnsFailureResponse()
    {
        // Arrange
        var updateRequest = new BLL.Song.UpdateSongRequest { Id = 99, Title = "Updated Song", ArtistId = 1 };
        var dalUpdateRequest = new DAL.Song.UpdateSongRequest();

        _mapperMock
            .Setup(m => m.Map<DAL.Song.UpdateSongRequest>(updateRequest))
            .Returns(dalUpdateRequest);

        _repositoryMock
            .Setup(r => r.UpdateAsync(dalUpdateRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Song.UpdateSongResult { Success = false, ErrorMessage = "Song not found" });

        var request = new ManagerRequest<BLL.Song.UpdateSongRequest> { Data = updateRequest, UserId = "user-1" };

        // Act
        var response = await _sut.UpdateSongAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Song not found", response.ErrorMessage);
    }

    [Fact]
    public async Task UpdateSongAsync_SetsUserIdOnRequestData_BeforeMappingAndCalling()
    {
        // Arrange
        var updateRequest = new BLL.Song.UpdateSongRequest { Id = 1, Title = "Updated Song", ArtistId = 1 };
        var dalUpdateRequest = new DAL.Song.UpdateSongRequest();
        var dalDetails = new DAL.Song.SongDetails { Id = 1, Title = "Updated Song" };
        var bllDetails = new BLL.Song.SongDetails { Id = 1, Title = "Updated Song" };

        _mapperMock
            .Setup(m => m.Map<DAL.Song.UpdateSongRequest>(updateRequest))
            .Returns(dalUpdateRequest);

        _repositoryMock
            .Setup(r => r.UpdateAsync(dalUpdateRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Song.UpdateSongResult { Success = true, SongDetails = dalDetails });

        _mapperMock
            .Setup(m => m.Map<BLL.Song.SongDetails>(dalDetails))
            .Returns(bllDetails);

        var request = new ManagerRequest<BLL.Song.UpdateSongRequest> { Data = updateRequest, UserId = "user-99" };

        // Act
        await _sut.UpdateSongAsync(request);

        // Assert
        Assert.Equal("user-99", updateRequest.UserId);
    }

    // -------------------------------------------------------------------------
    // DeleteSongAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteSongAsync_WhenRepositoryReturnsSuccess_ReturnsTrue()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Song.DeleteSongResult { Success = true });

        var request = new ManagerRequest<int> { Data = 1, UserId = "user-1" };

        // Act
        var response = await _sut.DeleteSongAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.True(response.Data);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task DeleteSongAsync_WhenRepositoryReturnsFailure_ReturnsFalseWithErrorMessage()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Song.DeleteSongResult { Success = false, ErrorMessage = "Song not found" });

        var request = new ManagerRequest<int> { Data = 99, UserId = "user-1" };

        // Act
        var response = await _sut.DeleteSongAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.False(response.Data);
        Assert.Equal("Song not found", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // ListAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListAsync_WhenRepositoryReturnsSuccess_ReturnsMappedPagedResult()
    {
        // Arrange
        var listRequest = new BLL.Song.ListSongsRequest { PageNumber = 1, PageSize = 10 };
        var dalListRequest = new DAL.Song.ListSongsRequest();
        var dalSummaries = new List<DAL.Song.SongSummary>
        {
            new() { Id = 1, Title = "Song One", Artist = "Artist One" },
            new() { Id = 2, Title = "Song Two", Artist = "Artist Two" }
        };
        var bllSummaries = new List<BLL.Song.SongSummary>
        {
            new() { Id = 1, Title = "Song One", Artist = "Artist One" },
            new() { Id = 2, Title = "Song Two", Artist = "Artist Two" }
        };

        _mapperMock
            .Setup(m => m.Map<DAL.Song.ListSongsRequest>(listRequest))
            .Returns(dalListRequest);

        _repositoryMock
            .Setup(r => r.ListAsync(dalListRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListSongsResult
            {
                Success = true,
                Songs = dalSummaries,
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            });

        _mapperMock
            .Setup(m => m.Map<List<BLL.Song.SongSummary>>(dalSummaries))
            .Returns(bllSummaries);

        var request = new ManagerRequest<BLL.Song.ListSongsRequest> { Data = listRequest, UserId = "user-1" };

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
        var listRequest = new BLL.Song.ListSongsRequest { PageNumber = 1, PageSize = 10 };
        var dalListRequest = new DAL.Song.ListSongsRequest();

        _mapperMock
            .Setup(m => m.Map<DAL.Song.ListSongsRequest>(listRequest))
            .Returns(dalListRequest);

        _repositoryMock
            .Setup(r => r.ListAsync(dalListRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Song.ListSongsResult { Success = false, ErrorMessage = "Database error" });

        var request = new ManagerRequest<BLL.Song.ListSongsRequest> { Data = listRequest, UserId = "user-1" };

        // Act
        var response = await _sut.ListAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Database error", response.ErrorMessage);
    }
}