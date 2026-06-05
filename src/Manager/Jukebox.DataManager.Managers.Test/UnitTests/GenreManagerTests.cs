using AutoMapper;
using Jukebox.DataAccess.Contracts.Interfaces;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Genre;
using Microsoft.Extensions.Logging;
using Moq;
using DAL = Jukebox.DataAccess.Contracts.DataContracts;
using BLL = Jukebox.DataManager.Contracts.DataContracts;

namespace Jukebox.DataManager.Managers.Test.UnitTests;

public class GenreManagerTests
{
    private readonly Mock<IGenreRepositoryAccess> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<GenreManager>> _loggerMock;
    private readonly GenreManager _sut;

    public GenreManagerTests()
    {
        _repositoryMock = new Mock<IGenreRepositoryAccess>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<GenreManager>>();
        _sut = new GenreManager(_repositoryMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    // -------------------------------------------------------------------------
    // FindByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task FindByIdAsync_WhenRepositoryReturnsSuccess_ReturnsMappedGenreDetails()
    {
        // Arrange
        var dalDetails = new DAL.Genre.GenreDetails { Id = 1, Name = "Rock" };
        var bllDetails = new BLL.Genre.GenreDetails { Id = 1, Name = "Rock" };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Genre.GetGenreResult { Success = true, GenreDetails = dalDetails });

        _mapperMock
            .Setup(m => m.Map<BLL.Genre.GenreDetails>(dalDetails))
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
            .ReturnsAsync(new DAL.Genre.GetGenreResult { Success = false, ErrorMessage = "Genre not found" });

        var request = new ManagerRequest<int> { Data = 99, UserId = "user-1" };

        // Act
        var response = await _sut.FindByIdAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Genre not found", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // AddGenreAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddGenreAsync_WhenRepositoryReturnsSuccess_ReturnsGenreSummaryWithId()
    {
        // Arrange
        var addRequest = new BLL.Genre.AddGenreRequest { Name = "Jazz" };
        var dalAddRequest = new DAL.Genre.AddGenreRequest { Name = "Jazz" };

        _mapperMock
            .Setup(m => m.Map<DAL.Genre.AddGenreRequest>(addRequest))
            .Returns(dalAddRequest);

        _repositoryMock
            .Setup(r => r.AddAsync(dalAddRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Genre.AddGenreResult { Success = true, GenreId = 42 });

        var request = new ManagerRequest<BLL.Genre.AddGenreRequest> { Data = addRequest, UserId = "user-1" };

        // Act
        var response = await _sut.AddGenreAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(42, response.Data.Id);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task AddGenreAsync_WhenRepositoryReturnsFailure_ReturnsFailureResponse()
    {
        // Arrange
        var addRequest = new BLL.Genre.AddGenreRequest { Name = "Jazz" };
        var dalAddRequest = new DAL.Genre.AddGenreRequest { Name = "Jazz" };

        _mapperMock
            .Setup(m => m.Map<DAL.Genre.AddGenreRequest>(addRequest))
            .Returns(dalAddRequest);

        _repositoryMock
            .Setup(r => r.AddAsync(dalAddRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Genre.AddGenreResult { Success = false, ErrorMessage = "Parent genre not found" });

        var request = new ManagerRequest<BLL.Genre.AddGenreRequest> { Data = addRequest, UserId = "user-1" };

        // Act
        var response = await _sut.AddGenreAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Parent genre not found", response.ErrorMessage);
    }

    [Fact]
    public async Task AddGenreAsync_SetsUserIdOnRequestData_BeforeMappingAndCalling()
    {
        // Arrange
        var addRequest = new BLL.Genre.AddGenreRequest { Name = "Jazz" };
        var dalAddRequest = new DAL.Genre.AddGenreRequest { Name = "Jazz" };

        _mapperMock
            .Setup(m => m.Map<DAL.Genre.AddGenreRequest>(addRequest))
            .Returns(dalAddRequest);

        _repositoryMock
            .Setup(r => r.AddAsync(dalAddRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Genre.AddGenreResult { Success = true, GenreId = 1 });

        var request = new ManagerRequest<BLL.Genre.AddGenreRequest> { Data = addRequest, UserId = "user-99" };

        // Act
        await _sut.AddGenreAsync(request);

        // Assert
        Assert.Equal("user-99", addRequest.UserId);
    }

    [Fact]
    public async Task AddGenreAsync_WhenNameIsEmpty_ReturnsFailureWithoutCallingRepository()
    {
        // Arrange
        var addRequest = new BLL.Genre.AddGenreRequest { Name = "   " };
        var request = new ManagerRequest<BLL.Genre.AddGenreRequest> { Data = addRequest, UserId = "user-1" };

        // Act
        var response = await _sut.AddGenreAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
        Assert.Contains("Name", response.ErrorMessage);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<DAL.Genre.AddGenreRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // -------------------------------------------------------------------------
    // UpdateGenreAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateGenreAsync_WhenRepositoryReturnsSuccess_ReturnsMappedGenreDetails()
    {
        // Arrange
        var updateRequest = new BLL.Genre.UpdateGenreRequest { Id = 1, Name = "Updated Rock" };
        var dalUpdateRequest = new DAL.Genre.UpdateGenreRequest { Id = 1, Name = "Updated Rock" };
        var dalDetails = new DAL.Genre.GenreDetails { Id = 1, Name = "Updated Rock" };
        var bllDetails = new BLL.Genre.GenreDetails { Id = 1, Name = "Updated Rock" };

        _mapperMock
            .Setup(m => m.Map<DAL.Genre.UpdateGenreRequest>(updateRequest))
            .Returns(dalUpdateRequest);

        _repositoryMock
            .Setup(r => r.UpdateAsync(dalUpdateRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Genre.UpdateGenreResult { Success = true, GenreDetails = dalDetails });

        _mapperMock
            .Setup(m => m.Map<BLL.Genre.GenreDetails>(dalDetails))
            .Returns(bllDetails);

        var request = new ManagerRequest<BLL.Genre.UpdateGenreRequest> { Data = updateRequest, UserId = "user-1" };

        // Act
        var response = await _sut.UpdateGenreAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(bllDetails, response.Data);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task UpdateGenreAsync_WhenRepositoryReturnsFailure_ReturnsFailureResponse()
    {
        // Arrange
        var updateRequest = new BLL.Genre.UpdateGenreRequest { Id = 99, Name = "Updated Rock" };
        var dalUpdateRequest = new DAL.Genre.UpdateGenreRequest { Id = 99, Name = "Updated Rock" };

        _mapperMock
            .Setup(m => m.Map<DAL.Genre.UpdateGenreRequest>(updateRequest))
            .Returns(dalUpdateRequest);

        _repositoryMock
            .Setup(r => r.UpdateAsync(dalUpdateRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Genre.UpdateGenreResult { Success = false, ErrorMessage = "Genre not found" });

        var request = new ManagerRequest<BLL.Genre.UpdateGenreRequest> { Data = updateRequest, UserId = "user-1" };

        // Act
        var response = await _sut.UpdateGenreAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Genre not found", response.ErrorMessage);
    }

    [Fact]
    public async Task UpdateGenreAsync_SetsUserIdOnRequestData_BeforeMappingAndCalling()
    {
        // Arrange
        var updateRequest = new BLL.Genre.UpdateGenreRequest { Id = 1, Name = "Updated Rock" };
        var dalUpdateRequest = new DAL.Genre.UpdateGenreRequest { Id = 1, Name = "Updated Rock" };
        var dalDetails = new DAL.Genre.GenreDetails { Id = 1, Name = "Updated Rock" };
        var bllDetails = new BLL.Genre.GenreDetails { Id = 1, Name = "Updated Rock" };

        _mapperMock
            .Setup(m => m.Map<DAL.Genre.UpdateGenreRequest>(updateRequest))
            .Returns(dalUpdateRequest);

        _repositoryMock
            .Setup(r => r.UpdateAsync(dalUpdateRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Genre.UpdateGenreResult { Success = true, GenreDetails = dalDetails });

        _mapperMock
            .Setup(m => m.Map<BLL.Genre.GenreDetails>(dalDetails))
            .Returns(bllDetails);

        var request = new ManagerRequest<BLL.Genre.UpdateGenreRequest> { Data = updateRequest, UserId = "user-99" };

        // Act
        await _sut.UpdateGenreAsync(request);

        // Assert
        Assert.Equal("user-99", updateRequest.UserId);
    }

    [Fact]
    public async Task UpdateGenreAsync_WhenNameIsEmpty_ReturnsFailureWithoutCallingRepository()
    {
        // Arrange
        var updateRequest = new BLL.Genre.UpdateGenreRequest { Id = 1, Name = "" };
        var request = new ManagerRequest<BLL.Genre.UpdateGenreRequest> { Data = updateRequest, UserId = "user-1" };

        // Act
        var response = await _sut.UpdateGenreAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
        Assert.Contains("Name", response.ErrorMessage);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<DAL.Genre.UpdateGenreRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // -------------------------------------------------------------------------
    // DeleteGenreAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteGenreAsync_WhenRepositoryReturnsSuccess_ReturnsTrue()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Genre.DeleteGenreResult { Success = true });

        var request = new ManagerRequest<int> { Data = 1, UserId = "user-1" };

        // Act
        var response = await _sut.DeleteGenreAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.True(response.Data);
        Assert.Null(response.ErrorMessage);
    }

    [Fact]
    public async Task DeleteGenreAsync_WhenRepositoryReturnsFailure_ReturnsFalseWithErrorMessage()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.DeleteAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Genre.DeleteGenreResult { Success = false, ErrorMessage = "Genre not found" });

        var request = new ManagerRequest<int> { Data = 99, UserId = "user-1" };

        // Act
        var response = await _sut.DeleteGenreAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.False(response.Data);
        Assert.Equal("Genre not found", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // ListAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListAsync_WhenRepositoryReturnsSuccess_ReturnsMappedPagedResult()
    {
        // Arrange
        var listRequest = new BLL.Genre.ListGenresRequest { PageNumber = 1, PageSize = 10 };
        var dalListRequest = new DAL.Genre.ListGenresRequest();
        var dalSummaries = new List<DAL.Genre.GenreSummary>
        {
            new() { Id = 1, Name = "Rock" },
            new() { Id = 2, Name = "Jazz" }
        };
        var bllSummaries = new List<BLL.Genre.GenreSummary>
        {
            new() { Id = 1, Name = "Rock" },
            new() { Id = 2, Name = "Jazz" }
        };

        _mapperMock
            .Setup(m => m.Map<DAL.Genre.ListGenresRequest>(listRequest))
            .Returns(dalListRequest);

        _repositoryMock
            .Setup(r => r.ListAsync(dalListRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Genre.ListGenresResult
            {
                Success = true,
                Genres = dalSummaries,
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            });

        _mapperMock
            .Setup(m => m.Map<List<BLL.Genre.GenreSummary>>(dalSummaries))
            .Returns(bllSummaries);

        var request = new ManagerRequest<BLL.Genre.ListGenresRequest> { Data = listRequest, UserId = "user-1" };

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
        var listRequest = new BLL.Genre.ListGenresRequest { PageNumber = 1, PageSize = 10 };
        var dalListRequest = new DAL.Genre.ListGenresRequest();

        _mapperMock
            .Setup(m => m.Map<DAL.Genre.ListGenresRequest>(listRequest))
            .Returns(dalListRequest);

        _repositoryMock
            .Setup(r => r.ListAsync(dalListRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DAL.Genre.ListGenresResult { Success = false, ErrorMessage = "Database error" });

        var request = new ManagerRequest<BLL.Genre.ListGenresRequest> { Data = listRequest, UserId = "user-1" };

        // Act
        var response = await _sut.ListAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Database error", response.ErrorMessage);
    }
}