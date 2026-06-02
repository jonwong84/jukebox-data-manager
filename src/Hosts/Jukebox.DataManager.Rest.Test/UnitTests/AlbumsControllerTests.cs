using Jukebox.DataManager.Contracts.DataContracts.Album;
using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Managers.Interfaces;
using Jukebox.DataManager.Rest.Controllers;
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
    public class AlbumsControllerTests
    {
        private readonly Mock<IAlbumManager> _mockAlbumManager;
        private readonly AlbumsController _controller;

        public AlbumsControllerTests()
        {
            _mockAlbumManager = new Mock<IAlbumManager>();
            _controller = new AlbumsController(_mockAlbumManager.Object);

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

        // GetAlbum
        [Fact]
        public async Task GetAlbum_ReturnsOk_WhenAlbumExists()
        {
            var albumDetails = new AlbumDetails { Id = 1, Title = "Test Album" };
            _mockAlbumManager
                .Setup(m => m.FindByIdAsync(It.IsAny<ManagerRequest<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<AlbumDetails> { Success = true, Data = albumDetails });

            var result = await _controller.GetAlbum(1, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(albumDetails, okResult.Value);
        }

        [Fact]
        public async Task GetAlbum_ReturnsNotFound_WhenAlbumDoesNotExist()
        {
            _mockAlbumManager
                .Setup(m => m.FindByIdAsync(It.IsAny<ManagerRequest<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<AlbumDetails> { Success = false, ErrorMessage = "Album not found" });

            var result = await _controller.GetAlbum(1, CancellationToken.None);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Album not found", notFoundResult.Value);
        }

        // ListAlbums
        [Fact]
        public async Task ListAlbums_ReturnsOk_WithPagedResult()
        {
            var pagedResult = new PagedResult<AlbumSummary>
            {
                Items = new List<AlbumSummary> { new AlbumSummary { Id = 1, Title = "Test Album" } },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            };
            _mockAlbumManager
                .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<ListAlbumsRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<PagedResult<AlbumSummary>> { Success = true, Data = pagedResult });

            var result = await _controller.ListAlbums(new ListAlbumsRequest(), CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pagedResult, okResult.Value);
        }

        [Fact]
        public async Task ListAlbums_Returns500_WhenListFails()
        {
            _mockAlbumManager
                .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<ListAlbumsRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<PagedResult<AlbumSummary>> { Success = false, ErrorMessage = "List failed" });

            var result = await _controller.ListAlbums(new ListAlbumsRequest(), CancellationToken.None);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("List failed", statusResult.Value);
        }

        // AddAlbum
        [Fact]
        public async Task AddAlbum_ReturnsCreatedAtAction_WhenSuccessful()
        {
            var albumSummary = new AlbumSummary { Id = 1, Title = "Test Album" };
            _mockAlbumManager
                .Setup(m => m.AddAlbumAsync(It.IsAny<ManagerRequest<AddAlbumRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<AlbumSummary> { Success = true, Data = albumSummary });

            var result = await _controller.AddAlbum(new AddAlbumRequest(), CancellationToken.None);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetAlbum), createdResult.ActionName);
            Assert.Equal(albumSummary, createdResult.Value);
        }

        [Fact]
        public async Task AddAlbum_ReturnsBadRequest_WhenAddFails()
        {
            _mockAlbumManager
                .Setup(m => m.AddAlbumAsync(It.IsAny<ManagerRequest<AddAlbumRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<AlbumSummary> { Success = false, ErrorMessage = "Add failed" });

            var result = await _controller.AddAlbum(new AddAlbumRequest(), CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Add failed", badRequestResult.Value);
        }

        // UpdateAlbum
        [Fact]
        public async Task UpdateAlbum_ReturnsOk_WhenSuccessful()
        {
            var albumDetails = new AlbumDetails { Id = 1, Title = "Updated Album" };
            _mockAlbumManager
                .Setup(m => m.UpdateAlbumAsync(It.IsAny<ManagerRequest<UpdateAlbumRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<AlbumDetails> { Success = true, Data = albumDetails });

            var result = await _controller.UpdateAlbum(1, new UpdateAlbumRequest { Id = 1 }, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(albumDetails, okResult.Value);
        }

        [Fact]
        public async Task UpdateAlbum_ReturnsBadRequest_WhenIdMismatch()
        {
            var result = await _controller.UpdateAlbum(1, new UpdateAlbumRequest { Id = 2 }, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Route id does not match request id", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateAlbum_ReturnsBadRequest_WhenUpdateFails()
        {
            _mockAlbumManager
                .Setup(m => m.UpdateAlbumAsync(It.IsAny<ManagerRequest<UpdateAlbumRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<AlbumDetails> { Success = false, ErrorMessage = "Album not found" });

            var result = await _controller.UpdateAlbum(1, new UpdateAlbumRequest { Id = 1 }, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Album not found", badRequestResult.Value);
        }

        // DeleteAlbum
        [Fact]
        public async Task DeleteAlbum_ReturnsNoContent_WhenSuccessful()
        {
            _mockAlbumManager
                .Setup(m => m.DeleteAlbumAsync(It.IsAny<ManagerRequest<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<bool> { Success = true });

            var result = await _controller.DeleteAlbum(1, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAlbum_ReturnsBadRequest_WhenDeleteFails()
        {
            _mockAlbumManager
                .Setup(m => m.DeleteAlbumAsync(It.IsAny<ManagerRequest<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<bool> { Success = false, ErrorMessage = "Album has songs" });

            var result = await _controller.DeleteAlbum(1, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Album has songs", badRequestResult.Value);
        }
    }
}
