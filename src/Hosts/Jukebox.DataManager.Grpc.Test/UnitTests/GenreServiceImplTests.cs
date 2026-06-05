using Grpc.Core;
using Grpc.Core.Testing;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Grpc.Services;
using Jukebox.DataManager.Managers.Interfaces;
using Moq;
using BLL = Jukebox.DataManager.Contracts.DataContracts;
using GrpcGenre = Jukebox.DataManager.Grpc.Genre;

namespace Jukebox.DataManager.Grpc.Test;

public class GenreServiceImplTests
{
    private readonly Mock<IGenreManager> _mockGenreManager;
    private readonly GenreServiceImpl _sut;
    private readonly ServerCallContext _callContext;

    public GenreServiceImplTests()
    {
        _mockGenreManager = new Mock<IGenreManager>();
        _sut = new GenreServiceImpl(_mockGenreManager.Object);
        _callContext = TestServerCallContext.Create(
            method: "TestMethod",
            host: "localhost",
            deadline: DateTime.MaxValue,
            requestHeaders: new Metadata(),
            cancellationToken: CancellationToken.None,
            peer: "127.0.0.1",
            authContext: null,
            contextPropagationToken: null,
            writeHeadersFunc: _ => Task.CompletedTask,
            writeOptionsGetter: () => null,
            writeOptionsSetter: _ => { });

        _callContext.UserState["userId"] = "test-user";
    }

    // -------------------------------------------------------------------------
    // GetGenre
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetGenre_ReturnsSuccess_WhenGenreExists()
    {
        // Arrange
        var genreDetails = new BLL.Genre.GenreDetails { Id = 1, Name = "Rock", Description = "Guitar-driven music." };

        _mockGenreManager
            .Setup(m => m.FindByIdAsync(It.Is<ManagerRequest<int>>(r => r.Data == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Genre.GenreDetails>
            {
                Success = true,
                Data = genreDetails,
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcGenre.GetGenreRequest { Id = 1 };

        // Act
        var response = await _sut.GetGenre(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(1, response.Genre.Id);
        Assert.Equal("Rock", response.Genre.Name);
        Assert.Equal("Guitar-driven music.", response.Genre.Description);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task GetGenre_ReturnsFailure_WhenGenreDoesNotExist()
    {
        // Arrange
        _mockGenreManager
            .Setup(m => m.FindByIdAsync(It.Is<ManagerRequest<int>>(r => r.Data == 99), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Genre.GenreDetails>
            {
                Success = false,
                ErrorMessage = "Genre not found.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcGenre.GetGenreRequest { Id = 99 };

        // Act
        var response = await _sut.GetGenre(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Genre);
        Assert.Equal("Genre not found.", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // ListGenres
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListGenres_ReturnsSuccess_WithPagedResult()
    {
        // Arrange
        var genres = new List<BLL.Genre.GenreSummary>
        {
            new() { Id = 1, Name = "Rock" },
            new() { Id = 2, Name = "Jazz" }
        };

        _mockGenreManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<BLL.Genre.ListGenresRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<BLL.Genre.GenreSummary>>
            {
                Success = true,
                Data = new PagedResult<BLL.Genre.GenreSummary>
                {
                    Items = genres,
                    TotalCount = 2,
                    Page = 1,
                    PageSize = 20
                },
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcGenre.ListGenresRequest
        {
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationRequest { Page = 1, PageSize = 20 }
        };

        // Act
        var response = await _sut.ListGenres(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(2, response.Genres.Count);
        Assert.Equal("Rock", response.Genres[0].Name);
        Assert.Equal(2, response.Pagination.TotalCount);
        Assert.Equal(1, response.Pagination.TotalPages);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task ListGenres_ReturnsFailure_WhenListFails()
    {
        // Arrange
        _mockGenreManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<BLL.Genre.ListGenresRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<BLL.Genre.GenreSummary>>
            {
                Success = false,
                ErrorMessage = "List operation failed.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcGenre.ListGenresRequest
        {
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationRequest { Page = 1, PageSize = 20 }
        };

        // Act
        var act = _sut.ListGenres(request, _callContext);

        // Assert
        var ex = await Assert.ThrowsAsync<RpcException>(() => act);
        Assert.Equal(StatusCode.Internal, ex.StatusCode);
        Assert.Equal("List operation failed.", ex.Status.Detail);
    }

    [Fact]
    public async Task ListGenres_ComputesTotalPagesCorrectly_WhenResultDoesNotDivideEvenly()
    {
        // Arrange
        _mockGenreManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<BLL.Genre.ListGenresRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<BLL.Genre.GenreSummary>>
            {
                Success = true,
                Data = new PagedResult<BLL.Genre.GenreSummary>
                {
                    Items = new List<BLL.Genre.GenreSummary>(),
                    TotalCount = 25,
                    Page = 1,
                    PageSize = 20
                },
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcGenre.ListGenresRequest
        {
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationRequest { Page = 1, PageSize = 20 }
        };

        // Act
        var response = await _sut.ListGenres(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(25, response.Pagination.TotalCount);
        Assert.Equal(2, response.Pagination.TotalPages); // Math.Ceiling(25 / 20.0) = 2
    }

    [Fact]
    public async Task ListGenres_PassesOptionalFilters_ToManager()
    {
        // Arrange
        _mockGenreManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<BLL.Genre.ListGenresRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<BLL.Genre.GenreSummary>>
            {
                Success = true,
                Data = new PagedResult<BLL.Genre.GenreSummary>
                {
                    Items = new List<BLL.Genre.GenreSummary>(),
                    TotalCount = 0,
                    Page = 1,
                    PageSize = 20
                },
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcGenre.ListGenresRequest
        {
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationRequest { Page = 1, PageSize = 20 },
            NameSearch = "rock",
            ParentGenreId = 5
        };

        // Act
        await _sut.ListGenres(request, _callContext);

        // Assert
        _mockGenreManager.Verify(m => m.ListAsync(
            It.Is<ManagerRequest<BLL.Genre.ListGenresRequest>>(r =>
                r.Data.NameSearch == "rock" &&
                r.Data.ParentGenreId == 5),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // -------------------------------------------------------------------------
    // CreateGenre
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateGenre_ReturnsSuccess_WhenGenreIsCreated()
    {
        // Arrange
        var genreDetails = new BLL.Genre.GenreDetails { Id = 42, Name = "Jazz" };

        _mockGenreManager
            .Setup(m => m.AddGenreAsync(It.IsAny<ManagerRequest<BLL.Genre.AddGenreRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Genre.GenreSummary>
            {
                Success = true,
                Data = new BLL.Genre.GenreSummary { Id = 42, Name = "Jazz" },
                ResponseTime = DateTime.UtcNow
            });

        _mockGenreManager
            .Setup(m => m.FindByIdAsync(It.Is<ManagerRequest<int>>(r => r.Data == 42), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Genre.GenreDetails>
            {
                Success = true,
                Data = genreDetails,
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcGenre.CreateGenreRequest { Name = "Jazz" };

        // Act
        var response = await _sut.CreateGenre(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(42, response.Genre.Id);
        Assert.Equal("Jazz", response.Genre.Name);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task CreateGenre_ReturnsFailure_WhenAddFails()
    {
        // Arrange
        _mockGenreManager
            .Setup(m => m.AddGenreAsync(It.IsAny<ManagerRequest<BLL.Genre.AddGenreRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Genre.GenreSummary>
            {
                Success = false,
                ErrorMessage = "Parent genre not found.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcGenre.CreateGenreRequest { Name = "Jazz", ParentGenreId = 999 };

        // Act
        var response = await _sut.CreateGenre(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Genre);
        Assert.Equal("Parent genre not found.", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // UpdateGenre
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateGenre_ReturnsSuccess_WhenGenreIsUpdated()
    {
        // Arrange
        var updatedDetails = new BLL.Genre.GenreDetails { Id = 1, Name = "Updated Rock" };

        _mockGenreManager
            .Setup(m => m.UpdateGenreAsync(It.IsAny<ManagerRequest<BLL.Genre.UpdateGenreRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Genre.GenreDetails>
            {
                Success = true,
                Data = updatedDetails,
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcGenre.UpdateGenreRequest { Id = 1, Name = "Updated Rock" };

        // Act
        var response = await _sut.UpdateGenre(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(1, response.Genre.Id);
        Assert.Equal("Updated Rock", response.Genre.Name);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task UpdateGenre_ReturnsFailure_WhenGenreDoesNotExist()
    {
        // Arrange
        _mockGenreManager
            .Setup(m => m.UpdateGenreAsync(It.IsAny<ManagerRequest<BLL.Genre.UpdateGenreRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Genre.GenreDetails>
            {
                Success = false,
                ErrorMessage = "Genre not found.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcGenre.UpdateGenreRequest { Id = 99, Name = "Ghost Genre" };

        // Act
        var response = await _sut.UpdateGenre(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Genre);
        Assert.Equal("Genre not found.", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // DeleteGenre
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteGenre_ReturnsSuccess_WhenGenreIsDeleted()
    {
        // Arrange
        _mockGenreManager
            .Setup(m => m.DeleteGenreAsync(It.Is<ManagerRequest<int>>(r => r.Data == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<bool>
            {
                Success = true,
                Data = true,
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcGenre.DeleteGenreRequest { Id = 1 };

        // Act
        var response = await _sut.DeleteGenre(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task DeleteGenre_ReturnsFailure_WhenDeleteFails()
    {
        // Arrange
        _mockGenreManager
            .Setup(m => m.DeleteGenreAsync(It.Is<ManagerRequest<int>>(r => r.Data == 99), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<bool>
            {
                Success = false,
                ErrorMessage = "Genre has sub-genres.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcGenre.DeleteGenreRequest { Id = 99 };

        // Act
        var response = await _sut.DeleteGenre(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Genre has sub-genres.", response.ErrorMessage);
    }
}