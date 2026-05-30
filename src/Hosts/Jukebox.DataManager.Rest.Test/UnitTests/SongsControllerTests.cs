using Jukebox.DataManager.Contracts.DataContracts.Common;
using Jukebox.DataManager.Contracts.DataContracts.Song;
using Jukebox.DataManager.Managers.Interfaces;
using Jukebox.DataManager.Rest.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Jukebox.DataManager.Rest.Test.UnitTests
{
    public class SongsControllerTests
    {
        private readonly Mock<ISongManager> _mockSongManager;
        private readonly Mock<ILogger<SongsController>> _mockLogger;
        private readonly SongsController _controller;

        public SongsControllerTests()
        {
            _mockSongManager = new Mock<ISongManager>();
            _mockLogger = new Mock<ILogger<SongsController>>();
            _controller = new SongsController(_mockSongManager.Object, _mockLogger.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        // GetSong
        [Fact]
        public async Task GetSong_ReturnsOk_WhenSongExists()
        {
            var songDetails = new SongDetails { Id = 1, Title = "Test Song" };
            _mockSongManager
                .Setup(m => m.FindByIdAsync(It.IsAny<ManagerRequest<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<SongDetails> { Success = true, Data = songDetails });

            var result = await _controller.GetSong(1, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(songDetails, okResult.Value);
        }

        [Fact]
        public async Task GetSong_ReturnsNotFound_WhenSongDoesNotExist()
        {
            _mockSongManager
                .Setup(m => m.FindByIdAsync(It.IsAny<ManagerRequest<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<SongDetails> { Success = false, ErrorMessage = "Song not found" });

            var result = await _controller.GetSong(1, CancellationToken.None);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Song not found", notFoundResult.Value);
        }

        // ListSongs
        [Fact]
        public async Task ListSongs_ReturnsOk_WithPagedResult()
        {
            var pagedResult = new PagedResult<SongSummary>
            {
                Items = new List<SongSummary> { new SongSummary { Id = 1, Title = "Test Song" } },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            };
            _mockSongManager
                .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<ListSongsRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<PagedResult<SongSummary>> { Success = true, Data = pagedResult });

            var result = await _controller.ListSongs(new ListSongsRequest(), CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pagedResult, okResult.Value);
        }

        [Fact]
        public async Task ListSongs_Returns500_WhenListFails()
        {
            _mockSongManager
                .Setup(m => m.ListAsync(It.IsAny<ManagerRequest<ListSongsRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<PagedResult<SongSummary>> { Success = false, ErrorMessage = "List failed" });

            var result = await _controller.ListSongs(new ListSongsRequest(), CancellationToken.None);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
            Assert.Equal("List failed", statusResult.Value);
        }

        // AddSong
        [Fact]
        public async Task AddSong_ReturnsCreatedAtAction_WhenSuccessful()
        {
            var songSummary = new SongSummary { Id = 1, Title = "Test Song" };
            _mockSongManager
                .Setup(m => m.AddSongAsync(It.IsAny<ManagerRequest<AddSongRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<SongSummary> { Success = true, Data = songSummary });

            var result = await _controller.AddSong(new AddSongRequest(), CancellationToken.None);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetSong), createdResult.ActionName);
            Assert.Equal(songSummary, createdResult.Value);
        }

        [Fact]
        public async Task AddSong_ReturnsBadRequest_WhenAddFails()
        {
            _mockSongManager
                .Setup(m => m.AddSongAsync(It.IsAny<ManagerRequest<AddSongRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<SongSummary> { Success = false, ErrorMessage = "Artist not found" });

            var result = await _controller.AddSong(new AddSongRequest(), CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Artist not found", badRequestResult.Value);
        }

        // UpdateSong
        [Fact]
        public async Task UpdateSong_ReturnsOk_WhenSuccessful()
        {
            var songDetails = new SongDetails { Id = 1, Title = "Updated Song" };
            _mockSongManager
                .Setup(m => m.UpdateSongAsync(It.IsAny<ManagerRequest<UpdateSongRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<SongDetails> { Success = true, Data = songDetails });

            var result = await _controller.UpdateSong(1, new UpdateSongRequest { Id = 1 }, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(songDetails, okResult.Value);
        }

        [Fact]
        public async Task UpdateSong_ReturnsBadRequest_WhenIdMismatch()
        {
            var result = await _controller.UpdateSong(1, new UpdateSongRequest { Id = 2 }, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Route id does not match request id", badRequestResult.Value);
        }
        [Fact]
        public async Task UpdateSong_ReturnsBadRequest_WhenUpdateFails()
        {
            _mockSongManager
                .Setup(m => m.UpdateSongAsync(It.IsAny<ManagerRequest<UpdateSongRequest>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<SongDetails> { Success = false, ErrorMessage = "Song not found" });

            var result = await _controller.UpdateSong(1, new UpdateSongRequest { Id = 1 }, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Song not found", badRequestResult.Value);
        }

        // DeleteSong
        [Fact]
        public async Task DeleteSong_ReturnsNoContent_WhenSuccessful()
        {
            _mockSongManager
                .Setup(m => m.DeleteSongAsync(It.IsAny<ManagerRequest<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<bool> { Success = true });

            var result = await _controller.DeleteSong(1, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteSong_ReturnsBadRequest_WhenDeleteFails()
        {
            _mockSongManager
                .Setup(m => m.DeleteSongAsync(It.IsAny<ManagerRequest<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ManagerResponse<bool> { Success = false, ErrorMessage = "Song not found" });

            var result = await _controller.DeleteSong(1, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Song not found", badRequestResult.Value);
        }
    }
}
