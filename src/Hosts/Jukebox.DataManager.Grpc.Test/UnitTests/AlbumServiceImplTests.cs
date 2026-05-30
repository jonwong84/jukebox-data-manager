using Grpc.Core;
using Grpc.Core.Testing;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Grpc.Services;
using Jukebox.DataManager.Managers.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using BLL = Jukebox.DataManager.Contracts.DataContracts;
using GrpcAlbum = Jukebox.DataManager.Grpc.Album;

namespace Jukebox.DataManager.Grpc.Test;

public class AlbumServiceImplTests
{
    private readonly Mock<IAlbumManager> _mockAlbumManager;
    private readonly Mock<ILogger<AlbumServiceImpl>> _mockLogger;
    private readonly AlbumServiceImpl _sut;
    private readonly ServerCallContext _callContext;

    public AlbumServiceImplTests()
    {
        _mockAlbumManager = new Mock<IAlbumManager>();
        _mockLogger = new Mock<ILogger<AlbumServiceImpl>>();
        _sut = new AlbumServiceImpl(_mockAlbumManager.Object, _mockLogger.Object);
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
    }

    // -------------------------------------------------------------------------
    // GetAlbum
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetAlbum_ReturnsSuccess_WhenAlbumExists()
    {
        // Arrange
        var albumDetails = new BLL.Album.AlbumDetails
        {
            Id = 1,
            Title = "Test Album",
            Artists = new List<BLL.Artist.ArtistSummary> { new() { Id = 10, Name = "Test Artist" } },
            ReleaseDate = new DateTime(2020, 6, 1),
            CreatedAt = DateTime.UtcNow,
            IsCompilation = false,
            Description = "A great album."
        };

        _mockAlbumManager
            .Setup(m => m.FindByIdAsync(It.Is<ManagerRequest<int>>(r => r.Data == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Album.AlbumDetails>
            {
                Success = true,
                Data = albumDetails,
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcAlbum.GetAlbumRequest { Id = 1, UserId = "user-abc" };

        // Act
        var response = await _sut.GetAlbum(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(1, response.Album.Id);
        Assert.Equal("Test Album", response.Album.Title);
        Assert.Single(response.Album.Artists);
        Assert.Equal("Test Artist", response.Album.Artists[0].Name);
        Assert.False(response.Album.IsCompilation);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task GetAlbum_ReturnsFailure_WhenAlbumDoesNotExist()
    {
        // Arrange
        _mockAlbumManager
            .Setup(m => m.FindByIdAsync(It.Is<ManagerRequest<int>>(r => r.Data == 99), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Album.AlbumDetails>
            {
                Success = false,
                Data = null,
                ErrorMessage = "Album not found.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcAlbum.GetAlbumRequest { Id = 99, UserId = "user-abc" };

        // Act
        var response = await _sut.GetAlbum(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Album);
        Assert.Equal("Album not found.", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // ListAlbums
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListAlbums_ReturnsSuccess_WithPagedResult()
    {
        // Arrange
        var albums = new List<BLL.Album.AlbumSummary>
        {
            new() { Id = 1, Title = "Album A", Artists = new List<BLL.Artist.ArtistSummary> { new() { Id = 1, Name = "Artist A" } } },
            new() { Id = 2, Title = "Album B", Artists = new List<BLL.Artist.ArtistSummary> { new() { Id = 2, Name = "Artist B" } } }
        };

        _mockAlbumManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<BLL.Album.ListAlbumsRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<BLL.Album.AlbumSummary>>
            {
                Success = true,
                Data = new PagedResult<BLL.Album.AlbumSummary>
                {
                    Items = albums,
                    TotalCount = 2,
                    Page = 1,
                    PageSize = 10
                },
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcAlbum.ListAlbumsRequest
        {
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationRequest { Page = 1, PageSize = 10 },
            UserId = "user-abc"
        };

        // Act
        var response = await _sut.ListAlbums(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(2, response.Albums.Count);
        Assert.Equal("Album A", response.Albums[0].Title);
        Assert.Equal(2, response.Pagination.TotalCount);
        Assert.Equal(1, response.Pagination.TotalPages);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task ListAlbums_ReturnsFailure_WhenListFails()
    {
        // Arrange
        _mockAlbumManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<BLL.Album.ListAlbumsRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<BLL.Album.AlbumSummary>>
            {
                Success = false,
                ErrorMessage = "List operation failed.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcAlbum.ListAlbumsRequest
        {
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationRequest { Page = 1, PageSize = 10 },
            UserId = "user-abc"
        };

        // Act
        var act = _sut.ListAlbums(request, _callContext);

        // Assert
        var ex = await Assert.ThrowsAsync<RpcException>(() => act);
        Assert.Equal(StatusCode.Internal, ex.StatusCode);
        Assert.Equal("List operation failed.", ex.Status.Detail);
    }

    [Fact]
    public async Task ListAlbums_ComputesTotalPagesCorrectly_WhenResultDoesNotDivideEvenly()
    {
        // Arrange
        var albums = new List<BLL.Album.AlbumSummary>
        {
            new() { Id = 1, Title = "Album A", Artists = new List<BLL.Artist.ArtistSummary>() }
        };

        _mockAlbumManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<BLL.Album.ListAlbumsRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<BLL.Album.AlbumSummary>>
            {
                Success = true,
                Data = new PagedResult<BLL.Album.AlbumSummary>
                {
                    Items = albums,
                    TotalCount = 25,
                    Page = 1,
                    PageSize = 10
                },
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcAlbum.ListAlbumsRequest
        {
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationRequest { Page = 1, PageSize = 10 },
            UserId = "user-abc"
        };

        // Act
        var response = await _sut.ListAlbums(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(25, response.Pagination.TotalCount);
        Assert.Equal(3, response.Pagination.TotalPages); // Math.Ceiling(25 / 10.0) = 3
    }

    [Fact]
    public async Task ListAlbums_PassesOptionalFilters_ToManager()
    {
        // Arrange
        _mockAlbumManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<BLL.Album.ListAlbumsRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<BLL.Album.AlbumSummary>>
            {
                Success = true,
                Data = new PagedResult<BLL.Album.AlbumSummary>
                {
                    Items = new List<BLL.Album.AlbumSummary>(),
                    TotalCount = 0,
                    Page = 1,
                    PageSize = 10
                },
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcAlbum.ListAlbumsRequest
        {
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationRequest { Page = 1, PageSize = 10 },
            UserId = "user-abc",
            ArtistId = 5,
            GenreId = 2,
            TitleSearch = "greatest hits"
        };

        // Act
        await _sut.ListAlbums(request, _callContext);

        // Assert
        _mockAlbumManager.Verify(m => m.ListAsync(
            It.Is<ManagerRequest<BLL.Album.ListAlbumsRequest>>(r =>
                r.Data.ArtistId == 5 &&
                r.Data.GenreId == 2 &&
                r.Data.TitleSearch == "greatest hits"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // -------------------------------------------------------------------------
    // CreateAlbum
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateAlbum_ReturnsSuccess_WhenAlbumIsCreated()
    {
        // Arrange
        var albumDetails = new BLL.Album.AlbumDetails
        {
            Id = 42,
            Title = "New Album",
            Artists = new List<BLL.Artist.ArtistSummary> { new() { Id = 10, Name = "Test Artist" } },
            CreatedAt = DateTime.UtcNow,
            IsCompilation = false,
            Description = string.Empty
        };

        _mockAlbumManager
            .Setup(m => m.AddAlbumAsync(It.IsAny<ManagerRequest<BLL.Album.AddAlbumRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Album.AlbumSummary>
            {
                Success = true,
                Data = new BLL.Album.AlbumSummary
                {
                    Id = 42,
                    Title = "New Album",
                    Artists = new List<BLL.Artist.ArtistSummary> { new() { Id = 10, Name = "Test Artist" } }
                },
                ResponseTime = DateTime.UtcNow
            });

        _mockAlbumManager
            .Setup(m => m.FindByIdAsync(It.Is<ManagerRequest<int>>(r => r.Data == 42), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Album.AlbumDetails>
            {
                Success = true,
                Data = albumDetails,
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcAlbum.CreateAlbumRequest
        {
            Title = "New Album",
            IsCompilation = false,
            UserId = "user-abc"
        };
        request.ArtistIds.Add(10);

        // Act
        var response = await _sut.CreateAlbum(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(42, response.Album.Id);
        Assert.Equal("New Album", response.Album.Title);
        Assert.Single(response.Album.Artists);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task CreateAlbum_ReturnsFailure_WhenAddFails()
    {
        // Arrange
        _mockAlbumManager
            .Setup(m => m.AddAlbumAsync(It.IsAny<ManagerRequest<BLL.Album.AddAlbumRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Album.AlbumSummary>
            {
                Success = false,
                ErrorMessage = "Artist not found.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcAlbum.CreateAlbumRequest
        {
            Title = "New Album",
            IsCompilation = false,
            UserId = "user-abc"
        };
        request.ArtistIds.Add(999);

        // Act
        var response = await _sut.CreateAlbum(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Album);
        Assert.Equal("Artist not found.", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // UpdateAlbum
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAlbum_ReturnsSuccess_WhenAlbumIsUpdated()
    {
        // Arrange
        var updatedDetails = new BLL.Album.AlbumDetails
        {
            Id = 1,
            Title = "Updated Album",
            Artists = new List<BLL.Artist.ArtistSummary> { new() { Id = 10, Name = "Test Artist" } },
            ReleaseDate = new DateTime(2021, 1, 1),
            CreatedAt = DateTime.UtcNow,
            IsCompilation = false,
            Description = "Updated description."
        };

        _mockAlbumManager
            .Setup(m => m.UpdateAlbumAsync(It.IsAny<ManagerRequest<BLL.Album.UpdateAlbumRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Album.AlbumDetails>
            {
                Success = true,
                Data = updatedDetails,
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcAlbum.UpdateAlbumRequest
        {
            Id = 1,
            Title = "Updated Album",
            IsCompilation = false,
            UserId = "user-abc"
        };
        request.ArtistIds.Add(10);

        // Act
        var response = await _sut.UpdateAlbum(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(1, response.Album.Id);
        Assert.Equal("Updated Album", response.Album.Title);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAlbum_ReturnsFailure_WhenAlbumDoesNotExist()
    {
        // Arrange
        _mockAlbumManager
            .Setup(m => m.UpdateAlbumAsync(It.IsAny<ManagerRequest<BLL.Album.UpdateAlbumRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Album.AlbumDetails>
            {
                Success = false,
                ErrorMessage = "Album not found.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcAlbum.UpdateAlbumRequest
        {
            Id = 99,
            Title = "Ghost Album",
            IsCompilation = false,
            UserId = "user-abc"
        };
        request.ArtistIds.Add(10);

        // Act
        var response = await _sut.UpdateAlbum(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Album);
        Assert.Equal("Album not found.", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // DeleteAlbum
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAlbum_ReturnsSuccess_WhenAlbumIsDeleted()
    {
        // Arrange
        _mockAlbumManager
            .Setup(m => m.DeleteAlbumAsync(It.Is<ManagerRequest<int>>(r => r.Data == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<bool>
            {
                Success = true,
                Data = true,
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcAlbum.DeleteAlbumRequest { Id = 1, UserId = "user-abc" };

        // Act
        var response = await _sut.DeleteAlbum(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAlbum_ReturnsFailure_WhenAlbumHasSongs()
    {
        // Arrange
        _mockAlbumManager
            .Setup(m => m.DeleteAlbumAsync(It.Is<ManagerRequest<int>>(r => r.Data == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<bool>
            {
                Success = false,
                ErrorMessage = "Album has associated songs and cannot be deleted.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcAlbum.DeleteAlbumRequest { Id = 1, UserId = "user-abc" };

        // Act
        var response = await _sut.DeleteAlbum(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Album has associated songs and cannot be deleted.", response.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAlbum_ReturnsFailure_WhenAlbumDoesNotExist()
    {
        // Arrange
        _mockAlbumManager
            .Setup(m => m.DeleteAlbumAsync(It.Is<ManagerRequest<int>>(r => r.Data == 99), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<bool>
            {
                Success = false,
                ErrorMessage = "Album not found.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcAlbum.DeleteAlbumRequest { Id = 99, UserId = "user-abc" };

        // Act
        var response = await _sut.DeleteAlbum(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Album not found.", response.ErrorMessage);
    }
}
