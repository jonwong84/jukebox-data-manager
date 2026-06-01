using Jukebox.DataManager.Contracts.DataContracts.Artist;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Managers.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Jukebox.DataManager.Rest.Test.UnitTests
{
    public class ArtistsControllerTests
    {
        private readonly Mock<IArtistManager> _mockArtistManager;
        private readonly Mock<ILogger<ArtistsController>> _mockLogger;
        private readonly ArtistsController _controller;

        public ArtistsControllerTests()
        {
            _mockArtistManager = new Mock<IArtistManager>();
            _mockLogger = new Mock<ILogger<ArtistsController>>();
            _controller = new ArtistsController(_mockArtistManager.Object, _mockLogger.Object);

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

        // GetArtist
        [Fact]
        public async Task GetArtist_ReturnsOk_WhenArtistExists()
        {
            var artistDetails = new ArtistDetails { Id = 1, Name = "Test Artist" };
            _mockArtistManager
                .Setup(m => m.FindByIdAsync(It.IsAny<ManagerRequest<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<ArtistDetails> { Success = true, Data = artistDetails });

            var result = await _controller.GetArtist(1, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(artistDetails, okResult.Value);
        }

        [Fact]
        public async Task GetArtist_ReturnsNotFound_WhenArtistDoesNotExist()
        {
            _mockArtistManager
                .Setup(m => m.FindByIdAsync(It.IsAny<ManagerRequest<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<ArtistDetails> { Success = false, ErrorMessage = "Artist not found" });

            var result = await _controller.GetArtist(1, CancellationToken.None);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Artist not found", notFoundResult.Value);
        }

        // ListArtists
        [Fact]
        public async Task ListArtists_ReturnsOk_WithPagedResult()
        {
            var pagedResult = new PagedResult<ArtistSummary>
            {
                Items = new List<ArtistSummary> { new ArtistSummary { Id = 1, Name = "Test Artist" } },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            };
            _mockArtistManager
                .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<ListArtistsRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<PagedResult<ArtistSummary>> { Success = true, Data = pagedResult });

            var result = await _controller.ListArtists(new ListArtistsRequest(), CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pagedResult, okResult.Value);
        }

        [Fact]
        public async Task ListArtists_Returns500_WhenListFails()
        {
            _mockArtistManager
                .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<ListArtistsRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<PagedResult<ArtistSummary>> { Success = false, ErrorMessage = "List failed" });

            var result = await _controller.ListArtists(new ListArtistsRequest(), CancellationToken.None);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("List failed", statusResult.Value);
        }

        // AddArtist
        [Fact]
        public async Task AddArtist_ReturnsCreatedAtAction_WhenSuccessful()
        {
            var artistSummary = new ArtistSummary { Id = 1, Name = "Test Artist" };
            _mockArtistManager
                .Setup(m => m.AddArtistAsync(It.IsAny<ManagerRequest<AddArtistRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<ArtistSummary> { Success = true, Data = artistSummary });

            var result = await _controller.AddArtist(new AddArtistRequest(), CancellationToken.None);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetArtist), createdResult.ActionName);
            Assert.Equal(artistSummary, createdResult.Value);
        }

        [Fact]
        public async Task AddArtist_ReturnsBadRequest_WhenAddFails()
        {
            _mockArtistManager
                .Setup(m => m.AddArtistAsync(It.IsAny<ManagerRequest<AddArtistRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<ArtistSummary> { Success = false, ErrorMessage = "Add failed" });

            var result = await _controller.AddArtist(new AddArtistRequest(), CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Add failed", badRequestResult.Value);
        }

        // UpdateArtist
        [Fact]
        public async Task UpdateArtist_ReturnsOk_WhenSuccessful()
        {
            var artistDetails = new ArtistDetails { Id = 1, Name = "Updated Artist" };
            _mockArtistManager
                .Setup(m => m.UpdateArtistAsync(It.IsAny<ManagerRequest<UpdateArtistRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<ArtistDetails> { Success = true, Data = artistDetails });

            var result = await _controller.UpdateArtist(1, new UpdateArtistRequest { Id = 1 }, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(artistDetails, okResult.Value);
        }

        [Fact]
        public async Task UpdateArtist_ReturnsBadRequest_WhenIdMismatch()
        {
            var result = await _controller.UpdateArtist(1, new UpdateArtistRequest { Id = 2 }, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Route id does not match request id", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateArtist_ReturnsBadRequest_WhenUpdateFails()
        {
            _mockArtistManager
                .Setup(m => m.UpdateArtistAsync(It.IsAny<ManagerRequest<UpdateArtistRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<ArtistDetails> { Success = false, ErrorMessage = "Artist not found" });

            var result = await _controller.UpdateArtist(1, new UpdateArtistRequest { Id = 1 }, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Artist not found", badRequestResult.Value);
        }

        // DeleteArtist
        [Fact]
        public async Task DeleteArtist_ReturnsNoContent_WhenSuccessful()
        {
            _mockArtistManager
                .Setup(m => m.DeleteArtistAsync(It.IsAny<ManagerRequest<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<bool> { Success = true });

            var result = await _controller.DeleteArtist(1, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteArtist_ReturnsBadRequest_WhenDeleteFails()
        {
            _mockArtistManager
                .Setup(m => m.DeleteArtistAsync(It.IsAny<ManagerRequest<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<bool> { Success = false, ErrorMessage = "Artist has songs" });

            var result = await _controller.DeleteArtist(1, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Artist has songs", badRequestResult.Value);
        }
    }
}
