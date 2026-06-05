using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Genre;
using Jukebox.DataManager.Managers.Interfaces;
using Jukebox.DataManager.Rest.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace Jukebox.DataManager.Rest.Test.UnitTests;

public class GenresControllerTests
{
    private readonly Mock<IGenreManager> _mockGenreManager;
    private readonly GenresController _controller;

    public GenresControllerTests()
    {
        _mockGenreManager = new Mock<IGenreManager>();
        _controller = new GenresController(_mockGenreManager.Object);

        var claims = new[] { new Claim("sub", "test-user") };
        var identity = new ClaimsIdentity(claims, authenticationType: "Test");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }

    // -------------------------------------------------------------------------
    // GetGenre
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetGenre_ReturnsOk_WhenGenreExists()
    {
        var genreDetails = new GenreDetails { Id = 1, Name = "Rock" };
        _mockGenreManager
            .Setup(m => m.FindByIdAsync(It.IsAny<ManagerRequest<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<GenreDetails> { Success = true, Data = genreDetails });

        var result = await _controller.GetGenre(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(genreDetails, okResult.Value);
    }

    [Fact]
    public async Task GetGenre_ReturnsNotFound_WhenGenreDoesNotExist()
    {
        _mockGenreManager
            .Setup(m => m.FindByIdAsync(It.IsAny<ManagerRequest<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<GenreDetails> { Success = false, ErrorMessage = "Genre not found" });

        var result = await _controller.GetGenre(1, CancellationToken.None);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Genre not found", notFoundResult.Value);
    }

    // -------------------------------------------------------------------------
    // ListGenres
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListGenres_ReturnsOk_WithPagedResult()
    {
        var pagedResult = new PagedResult<GenreSummary>
        {
            Items = new List<GenreSummary> { new() { Id = 1, Name = "Rock" } },
            TotalCount = 1,
            Page = 1,
            PageSize = 20
        };
        _mockGenreManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<ListGenresRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<GenreSummary>> { Success = true, Data = pagedResult });

        var result = await _controller.ListGenres(new ListGenresRequest(), CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(pagedResult, okResult.Value);
    }

    [Fact]
    public async Task ListGenres_Returns500_WhenListFails()
    {
        _mockGenreManager
            .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<ListGenresRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<PagedResult<GenreSummary>> { Success = false, ErrorMessage = "List failed" });

        var result = await _controller.ListGenres(new ListGenresRequest(), CancellationToken.None);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal("List failed", statusResult.Value);
    }

    // -------------------------------------------------------------------------
    // AddGenre
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddGenre_ReturnsCreatedAtAction_WhenSuccessful()
    {
        var genreSummary = new GenreSummary { Id = 1, Name = "Rock" };
        _mockGenreManager
            .Setup(m => m.AddGenreAsync(It.IsAny<ManagerRequest<AddGenreRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<GenreSummary> { Success = true, Data = genreSummary });

        var result = await _controller.AddGenre(new AddGenreRequest { Name = "Rock" }, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetGenre), createdResult.ActionName);
        Assert.Equal(genreSummary, createdResult.Value);
    }

    [Fact]
    public async Task AddGenre_ReturnsBadRequest_WhenAddFails()
    {
        _mockGenreManager
            .Setup(m => m.AddGenreAsync(It.IsAny<ManagerRequest<AddGenreRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<GenreSummary> { Success = false, ErrorMessage = "Parent genre not found" });

        var result = await _controller.AddGenre(new AddGenreRequest { Name = "Rock" }, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Parent genre not found", badRequestResult.Value);
    }

    // -------------------------------------------------------------------------
    // UpdateGenre
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateGenre_ReturnsOk_WhenSuccessful()
    {
        var genreDetails = new GenreDetails { Id = 1, Name = "Updated Rock" };
        _mockGenreManager
            .Setup(m => m.UpdateGenreAsync(It.IsAny<ManagerRequest<UpdateGenreRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<GenreDetails> { Success = true, Data = genreDetails });

        var result = await _controller.UpdateGenre(1, new UpdateGenreRequest { Id = 1, Name = "Updated Rock" }, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(genreDetails, okResult.Value);
    }

    [Fact]
    public async Task UpdateGenre_ReturnsBadRequest_WhenIdMismatch()
    {
        var result = await _controller.UpdateGenre(1, new UpdateGenreRequest { Id = 2, Name = "Rock" }, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Route id does not match request id", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateGenre_ReturnsBadRequest_WhenUpdateFails()
    {
        _mockGenreManager
            .Setup(m => m.UpdateGenreAsync(It.IsAny<ManagerRequest<UpdateGenreRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<GenreDetails> { Success = false, ErrorMessage = "Genre not found" });

        var result = await _controller.UpdateGenre(1, new UpdateGenreRequest { Id = 1, Name = "Rock" }, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Genre not found", badRequestResult.Value);
    }

    // -------------------------------------------------------------------------
    // DeleteGenre
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteGenre_ReturnsNoContent_WhenSuccessful()
    {
        _mockGenreManager
            .Setup(m => m.DeleteGenreAsync(It.IsAny<ManagerRequest<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<bool> { Success = true });

        var result = await _controller.DeleteGenre(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteGenre_ReturnsBadRequest_WhenDeleteFails()
    {
        _mockGenreManager
            .Setup(m => m.DeleteGenreAsync(It.IsAny<ManagerRequest<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ManagerResponse<bool> { Success = false, ErrorMessage = "Genre has sub-genres." });

        var result = await _controller.DeleteGenre(1, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Genre has sub-genres.", badRequestResult.Value);
    }
}