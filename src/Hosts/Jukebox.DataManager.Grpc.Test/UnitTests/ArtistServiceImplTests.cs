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
using GrpcArtist = Jukebox.DataManager.Grpc.Artist;

namespace Jukebox.DataManager.Grpc.Test;

public class ArtistServiceImplTests
{
    private readonly Mock<IArtistManager> _mockArtistManager;
    private readonly Mock<ILogger<ArtistServiceImpl>> _mockLogger;
    private readonly ArtistServiceImpl _sut;
    private readonly ServerCallContext _callContext;

    public ArtistServiceImplTests()
    {
        _mockArtistManager = new Mock<IArtistManager>();
        _mockLogger = new Mock<ILogger<ArtistServiceImpl>>();
        _sut = new ArtistServiceImpl(_mockArtistManager.Object, _mockLogger.Object);
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
    // GetArtist
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetArtist_ReturnsSuccess_WhenArtistExists()
    {
        // Arrange
        var artistDetails = new BLL.Artist.ArtistDetails
        {
            Id = 1,
            Name = "Test Artist",
            Bio = "A great artist.",
            CreatedAt = DateTime.UtcNow,
            Albums = new List<BLL.Album.AlbumSummary>
            {
                new() { Id = 10, Title = "Album One", Artists = new List<BLL.Artist.ArtistSummary>() }
            }
        };

        _mockArtistManager
            .Setup(m => m.FindByIdAsync(It.Is<ManagerRequest<int>>(r => r.Data == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Artist.ArtistDetails>
            {
                Success = true,
                Data = artistDetails,
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcArtist.GetArtistRequest { Id = 1, UserId = "user-abc" };

        // Act
        var response = await _sut.GetArtist(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(1, response.Artist.Id);
        Assert.Equal("Test Artist", response.Artist.Name);
        Assert.Equal("A great artist.", response.Artist.Bio);
        Assert.Single(response.Artist.Albums);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task GetArtist_ReturnsFailure_WhenArtistDoesNotExist()
    {
        // Arrange
        _mockArtistManager
            .Setup(m => m.FindByIdAsync(It.Is<ManagerRequest<int>>(r => r.Data == 99), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Artist.ArtistDetails>
            {
                Success = false,
                Data = null,
                ErrorMessage = "Artist not found.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcArtist.GetArtistRequest { Id = 99, UserId = "user-abc" };

        // Act
        var response = await _sut.GetArtist(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Artist);
        Assert.Equal("Artist not found.", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // ListArtists
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListArtists_ReturnsSuccess_WithPagedResult()
    {
        // Arrange
        var artists = new List<BLL.Artist.ArtistSummary>
        {
            new() { Id = 1, Name = "Artist A" },
            new() { Id = 2, Name = "Artist B" }
        };

        _mockArtistManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<BLL.Artist.ListArtistsRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<BLL.Artist.ArtistSummary>>
            {
                Success = true,
                Data = new PagedResult<BLL.Artist.ArtistSummary>
                {
                    Items = artists,
                    TotalCount = 2,
                    Page = 1,
                    PageSize = 10
                },
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcArtist.ListArtistsRequest
        {
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationRequest { Page = 1, PageSize = 10 },
            UserId = "user-abc"
        };

        // Act
        var response = await _sut.ListArtists(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(2, response.Artists.Count);
        Assert.Equal("Artist A", response.Artists[0].Name);
        Assert.Equal(2, response.Pagination.TotalCount);
        Assert.Equal(1, response.Pagination.TotalPages);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task ListArtists_ReturnsFailure_WhenListFails()
    {
        // Arrange
        _mockArtistManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<BLL.Artist.ListArtistsRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<BLL.Artist.ArtistSummary>>
            {
                Success = false,
                ErrorMessage = "List operation failed.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcArtist.ListArtistsRequest
        {
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationRequest { Page = 1, PageSize = 10 },
            UserId = "user-abc"
        };

        // Act
        var act = _sut.ListArtists(request, _callContext);

        // Assert
        var ex = await Assert.ThrowsAsync<RpcException>(() => act);
        Assert.Equal(StatusCode.Internal, ex.StatusCode);
        Assert.Equal("List operation failed.", ex.Status.Detail);
    }

    [Fact]
    public async Task ListArtists_ComputesTotalPagesCorrectly_WhenResultDoesNotDivideEvenly()
    {
        // Arrange
        var artists = new List<BLL.Artist.ArtistSummary> { new() { Id = 1, Name = "Artist A" } };

        _mockArtistManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<BLL.Artist.ListArtistsRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<BLL.Artist.ArtistSummary>>
            {
                Success = true,
                Data = new PagedResult<BLL.Artist.ArtistSummary>
                {
                    Items = artists,
                    TotalCount = 25,
                    Page = 1,
                    PageSize = 10
                },
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcArtist.ListArtistsRequest
        {
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationRequest { Page = 1, PageSize = 10 },
            UserId = "user-abc"
        };

        // Act
        var response = await _sut.ListArtists(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(25, response.Pagination.TotalCount);
        Assert.Equal(3, response.Pagination.TotalPages); // Math.Ceiling(25 / 10.0) = 3
    }

    [Fact]
    public async Task ListArtists_PassesOptionalFilters_ToManager()
    {
        // Arrange
        _mockArtistManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<BLL.Artist.ListArtistsRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<BLL.Artist.ArtistSummary>>
            {
                Success = true,
                Data = new PagedResult<BLL.Artist.ArtistSummary>
                {
                    Items = new List<BLL.Artist.ArtistSummary>(),
                    TotalCount = 0,
                    Page = 1,
                    PageSize = 10
                },
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcArtist.ListArtistsRequest
        {
            Pagination = new Jukebox.DataManager.Grpc.Common.PaginationRequest { Page = 1, PageSize = 10 },
            UserId = "user-abc",
            NameSearch = "Beatles"
        };

        // Act
        await _sut.ListArtists(request, _callContext);

        // Assert
        _mockArtistManager.Verify(m => m.ListAsync(
            It.Is<ManagerRequest<BLL.Artist.ListArtistsRequest>>(r =>
                r.Data.NameSearch == "Beatles"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // -------------------------------------------------------------------------
    // CreateArtist
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateArtist_ReturnsSuccess_WhenArtistIsCreated()
    {
        // Arrange
        var artistDetails = new BLL.Artist.ArtistDetails
        {
            Id = 42,
            Name = "New Artist",
            Bio = "Up and coming.",
            CreatedAt = DateTime.UtcNow,
            Albums = new List<BLL.Album.AlbumSummary>()
        };

        _mockArtistManager
            .Setup(m => m.AddArtistAsync(It.IsAny<ManagerRequest<BLL.Artist.AddArtistRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Artist.ArtistSummary>
            {
                Success = true,
                Data = new BLL.Artist.ArtistSummary { Id = 42, Name = "New Artist" },
                ResponseTime = DateTime.UtcNow
            });

        _mockArtistManager
            .Setup(m => m.FindByIdAsync(It.Is<ManagerRequest<int>>(r => r.Data == 42), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Artist.ArtistDetails>
            {
                Success = true,
                Data = artistDetails,
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcArtist.CreateArtistRequest
        {
            Name = "New Artist",
            Bio = "Up and coming.",
            UserId = "user-abc"
        };

        // Act
        var response = await _sut.CreateArtist(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(42, response.Artist.Id);
        Assert.Equal("New Artist", response.Artist.Name);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task CreateArtist_ReturnsFailure_WhenAddFails()
    {
        // Arrange
        _mockArtistManager
            .Setup(m => m.AddArtistAsync(It.IsAny<ManagerRequest<BLL.Artist.AddArtistRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Artist.ArtistSummary>
            {
                Success = false,
                ErrorMessage = "Artist already exists.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcArtist.CreateArtistRequest
        {
            Name = "Duplicate Artist",
            UserId = "user-abc"
        };

        // Act
        var response = await _sut.CreateArtist(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Artist);
        Assert.Equal("Artist already exists.", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // UpdateArtist
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateArtist_ReturnsSuccess_WhenArtistIsUpdated()
    {
        // Arrange
        var updatedDetails = new BLL.Artist.ArtistDetails
        {
            Id = 1,
            Name = "Updated Artist",
            Bio = "Updated bio.",
            CreatedAt = DateTime.UtcNow,
            Albums = new List<BLL.Album.AlbumSummary>()
        };

        _mockArtistManager
            .Setup(m => m.UpdateArtistAsync(It.IsAny<ManagerRequest<BLL.Artist.UpdateArtistRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Artist.ArtistDetails>
            {
                Success = true,
                Data = updatedDetails,
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcArtist.UpdateArtistRequest
        {
            Id = 1,
            Name = "Updated Artist",
            Bio = "Updated bio.",
            UserId = "user-abc"
        };

        // Act
        var response = await _sut.UpdateArtist(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(1, response.Artist.Id);
        Assert.Equal("Updated Artist", response.Artist.Name);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task UpdateArtist_ReturnsFailure_WhenArtistDoesNotExist()
    {
        // Arrange
        _mockArtistManager
            .Setup(m => m.UpdateArtistAsync(It.IsAny<ManagerRequest<BLL.Artist.UpdateArtistRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<BLL.Artist.ArtistDetails>
            {
                Success = false,
                ErrorMessage = "Artist not found.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcArtist.UpdateArtistRequest
        {
            Id = 99,
            Name = "Ghost Artist",
            UserId = "user-abc"
        };

        // Act
        var response = await _sut.UpdateArtist(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Null(response.Artist);
        Assert.Equal("Artist not found.", response.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // DeleteArtist
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteArtist_ReturnsSuccess_WhenArtistIsDeleted()
    {
        // Arrange
        _mockArtistManager
            .Setup(m => m.DeleteArtistAsync(It.Is<ManagerRequest<int>>(r => r.Data == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<bool>
            {
                Success = true,
                Data = true,
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcArtist.DeleteArtistRequest { Id = 1, UserId = "user-abc" };

        // Act
        var response = await _sut.DeleteArtist(request, _callContext);

        // Assert
        Assert.True(response.Success);
        Assert.Empty(response.ErrorMessage);
    }

    [Fact]
    public async Task DeleteArtist_ReturnsFailure_WhenArtistHasSongs()
    {
        // Arrange
        _mockArtistManager
            .Setup(m => m.DeleteArtistAsync(It.Is<ManagerRequest<int>>(r => r.Data == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<bool>
            {
                Success = false,
                ErrorMessage = "Artist has associated songs and cannot be deleted.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcArtist.DeleteArtistRequest { Id = 1, UserId = "user-abc" };

        // Act
        var response = await _sut.DeleteArtist(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Artist has associated songs and cannot be deleted.", response.ErrorMessage);
    }

    [Fact]
    public async Task DeleteArtist_ReturnsFailure_WhenArtistDoesNotExist()
    {
        // Arrange
        _mockArtistManager
            .Setup(m => m.DeleteArtistAsync(It.Is<ManagerRequest<int>>(r => r.Data == 99), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<bool>
            {
                Success = false,
                ErrorMessage = "Artist not found.",
                ResponseTime = DateTime.UtcNow
            });

        var request = new GrpcArtist.DeleteArtistRequest { Id = 99, UserId = "user-abc" };

        // Act
        var response = await _sut.DeleteArtist(request, _callContext);

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Artist not found.", response.ErrorMessage);
    }
}
