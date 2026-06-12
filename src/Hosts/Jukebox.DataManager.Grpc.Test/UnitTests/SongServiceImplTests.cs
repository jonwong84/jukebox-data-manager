using Grpc.Core;
using Grpc.Core.Testing;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Grpc.Genre;
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
using GrpcSong = Jukebox.DataManager.Grpc.Song;

namespace Jukebox.DataManager.Grpc.Test;

public class SongServiceImplTests
{
    private readonly Mock<ISongManager> _mockSongManager;
    private readonly SongServiceImpl _sut;
    private readonly ServerCallContext _callContext;

    public SongServiceImplTests()
    {
        _mockSongManager = new Mock<ISongManager>();
        _sut = new SongServiceImpl(_mockSongManager.Object);
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
    // GetSong
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetSong_ReturnsSuccess_WhenSongExists()
    {
        // Arrange
        var songDetails = new BLL.Song.SongDetails
        {
            Id = 1,
            Title = "Test Song",
            ArtistId = 10,
            Artist = new BLL.Artist.ArtistSummary { Id = 10, Name = "Test Artist" },
            Duration = TimeSpan.FromSeconds(210),
            Genres = new List<BLL.Genre.GenreSummary> { new() { Id = 1, Name = "Rock" } }
        };

        _mockSongManager
            .Setup(m => m.FindByIdAsync(It.Is<ManagerRequest<int>>(r => r.Data == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Song.SongDetails>
            {
                Success = true,
                Data = songDetails,
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcSong.GetSongRequest { Id = 1, UserId = "user-abc" };

        // Act
        var response = await _sut.GetSong(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(1, response.Song.Id);
        Assert.Equal("Test Song", response.Song.Title);
        Assert.Equal(songDetails.Duration.Ticks, response.Song.DurationTicks);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task GetSong_ReturnsFailure_WhenSongDoesNotExist()
    {
        // Arrange
        _mockSongManager
            .Setup(m => m.FindByIdAsync(It.Is<ManagerRequest<int>>(r => r.Data == 99), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Song.SongDetails>
            {
                Success = false,
                Data = null,
                ErrorMessage = "Song not found.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcSong.GetSongRequest { Id = 99, UserId = "user-abc" };

        // Act
        var response = await _sut.GetSong(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Song);
        Assert.Equal("Song not found.", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // ListSongs
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListSongs_ReturnsSuccess_WithPagedResult()
    {
        // Arrange
        var songs = new List<BLL.Song.SongSummary>
        {
            new() { Id = 1, Title = "Song A", Artist = "Artist A" },
            new() { Id = 2, Title = "Song B", Artist = "Artist B" }
        };

        _mockSongManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<BLL.Song.ListSongsRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<BLL.Song.SongSummary>>
            {
                Success = true,
                Data = new PagedResult<BLL.Song.SongSummary>
                {
                    Items = songs,
                    TotalCount = 2,
                    Page = 1,
                    PageSize = 10
                },
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcSong.ListSongsRequest
        {
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationRequest { Page = 1, PageSize = 10 },
            UserId = "user-abc"
        };

        // Act
        var response = await _sut.ListSongs(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(2, response.Songs.Count);
        Assert.Equal("Song A", response.Songs[0].Title);
        Assert.Equal(2, response.Pagination.TotalCount);
        Assert.Equal(1, response.Pagination.TotalPages);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task ListSongs_ReturnsFailure_WhenListFails()
    {
        // Arrange
        _mockSongManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<BLL.Song.ListSongsRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<BLL.Song.SongSummary>>
            {
                Success = false,
                ErrorMessage = "List operation failed.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcSong.ListSongsRequest
        {
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationRequest { Page = 1, PageSize = 10 },
            UserId = "user-abc"
        };

        // Act
        var act = _sut.ListSongs(request, _callContext);

        // Assert
        var ex = await Assert.ThrowsAsync<RpcException>(() => act);
        Assert.Equal(StatusCode.Internal, ex.StatusCode);
        Assert.Equal("List operation failed.", ex.Status.Detail);
    }

    [Fact]
    public async Task ListSongs_ComputesTotalPagesCorrectly_WhenResultDoesNotDivideEvenly()
    {
        // Arrange
        var songs = new List<BLL.Song.SongSummary> { new() { Id = 1, Title = "Song A", Artist = "Artist A" } };

        _mockSongManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<BLL.Song.ListSongsRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<BLL.Song.SongSummary>>
            {
                Success = true,
                Data = new PagedResult<BLL.Song.SongSummary>
                {
                    Items = songs,
                    TotalCount = 25,
                    Page = 1,
                    PageSize = 10
                },
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcSong.ListSongsRequest
        {
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationRequest { Page = 1, PageSize = 10 },
            UserId = "user-abc"
        };

        // Act
        var response = await _sut.ListSongs(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(25, response.Pagination.TotalCount);
        Assert.Equal(3, response.Pagination.TotalPages); // Math.Ceiling(25 / 10.0) = 3
    }

    [Fact]
    public async Task ListSongs_PassesOptionalFilters_ToManager()
    {
        // Arrange
        _mockSongManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<BLL.Song.ListSongsRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<BLL.Song.SongSummary>>
            {
                Success = true,
                Data = new PagedResult<BLL.Song.SongSummary>
                {
                    Items = new List<BLL.Song.SongSummary>(),
                    TotalCount = 0,
                    Page = 1,
                    PageSize = 10
                },
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcSong.ListSongsRequest
        {
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationRequest { Page = 1, PageSize = 10 },
            UserId = "user-abc",
            ArtistId = 5,
            AlbumId = 3,
            GenreId = 2,
            MinBpm = 100,
            MaxBpm = 140,
            TitleSearch = "rock"
        };

        // Act
        await _sut.ListSongs(request, _callContext);

        // Assert
        _mockSongManager.Verify(m => m.ListAsync(
            It.Is<ManagerRequest<BLL.Song.ListSongsRequest>>(r =>
                r.Data.ArtistId == 5 &&
                r.Data.AlbumId == 3 &&
                r.Data.GenreId == 2 &&
                r.Data.MinBpm == 100 &&
                r.Data.MaxBpm == 140 &&
                r.Data.TitleSearch == "rock"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // -------------------------------------------------------------------------
    // CreateSong
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateSong_ReturnsSuccess_WhenSongIsCreated()
    {
        // Arrange
        var songDetails = new BLL.Song.SongDetails
        {
            Id = 42,
            Title = "New Song",
            ArtistId = 10,
            Artist = new BLL.Artist.ArtistSummary { Id = 10, Name = "Test Artist" },
            Duration = TimeSpan.FromSeconds(180),
            Genres = new List<BLL.Genre.GenreSummary>()
        };

        _mockSongManager
            .Setup(m => m.AddSongAsync(It.IsAny<ManagerRequest<BLL.Song.AddSongRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Song.SongSummary>
            {
                Success = true,
                Data = new BLL.Song.SongSummary { Id = 42, Title = "New Song", Artist = "Test Artist" },
                ResponseTime = DateTime.UtcNow
            });

        _mockSongManager
            .Setup(m => m.FindByIdAsync(It.Is<ManagerRequest<int>>(r => r.Data == 42), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Song.SongDetails>
            {
                Success = true,
                Data = songDetails,
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcSong.CreateSongRequest
        {
            Title = "New Song",
            ArtistId = 10,
            DurationTicks = TimeSpan.FromSeconds(180).Ticks,
            UserId = "user-abc"
        };

        // Act
        var response = await _sut.CreateSong(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(42, response.Song.Id);
        Assert.Equal("New Song", response.Song.Title);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task CreateSong_ReturnsFailure_WhenAddFails()
    {
        // Arrange
        _mockSongManager
            .Setup(m => m.AddSongAsync(It.IsAny<ManagerRequest<BLL.Song.AddSongRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Song.SongSummary>
            {
                Success = false,
                ErrorMessage = "Artist not found.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcSong.CreateSongRequest
        {
            Title = "New Song",
            ArtistId = 999,
            DurationTicks = TimeSpan.FromSeconds(180).Ticks,
            UserId = "user-abc"
        };

        // Act
        var response = await _sut.CreateSong(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Song);
        Assert.Equal("Artist not found.", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // UpdateSong
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateSong_ReturnsSuccess_WhenSongIsUpdated()
    {
        // Arrange
        var updatedDetails = new BLL.Song.SongDetails
        {
            Id = 1,
            Title = "Updated Song",
            ArtistId = 10,
            Artist = new BLL.Artist.ArtistSummary { Id = 10, Name = "Test Artist" },
            Duration = TimeSpan.FromSeconds(200)
        };

        _mockSongManager
            .Setup(m => m.UpdateSongAsync(It.IsAny<ManagerRequest<BLL.Song.UpdateSongRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Song.SongDetails>
            {
                Success = true,
                Data = updatedDetails,
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcSong.UpdateSongRequest
        {
            Id = 1,
            Title = "Updated Song",
            ArtistId = 10,
            DurationTicks = TimeSpan.FromSeconds(200).Ticks,
            UserId = "user-abc"
        };

        // Act
        var response = await _sut.UpdateSong(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(1, response.Song.Id);
        Assert.Equal("Updated Song", response.Song.Title);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task UpdateSong_ReturnsFailure_WhenSongDoesNotExist()
    {
        // Arrange
        _mockSongManager
            .Setup(m => m.UpdateSongAsync(It.IsAny<ManagerRequest<BLL.Song.UpdateSongRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Song.SongDetails>
            {
                Success = false,
                ErrorMessage = "Song not found.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcSong.UpdateSongRequest
        {
            Id = 99,
            Title = "Ghost Song",
            ArtistId = 10,
            DurationTicks = TimeSpan.FromSeconds(180).Ticks,
            UserId = "user-abc"
        };

        // Act
        var response = await _sut.UpdateSong(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Song);
        Assert.Equal("Song not found.", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // DeleteSong
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteSong_ReturnsSuccess_WhenSongIsDeleted()
    {
        // Arrange
        _mockSongManager
            .Setup(m => m.DeleteSongAsync(It.Is<ManagerRequest<int>>(r => r.Data == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<bool>
            {
                Success = true,
                Data = true,
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcSong.DeleteSongRequest { Id = 1, UserId = "user-abc" };

        // Act
        var response = await _sut.DeleteSong(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task DeleteSong_ReturnsFailure_WhenDeleteFails()
    {
        // Arrange
        _mockSongManager
            .Setup(m => m.DeleteSongAsync(It.Is<ManagerRequest<int>>(r => r.Data == 99), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<bool>
            {
                Success = false,
                ErrorMessage = "Song not found.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcSong.DeleteSongRequest { Id = 99, UserId = "user-abc" };

        // Act
        var response = await _sut.DeleteSong(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Song not found.", response.ErrorMessage);
    }
}
