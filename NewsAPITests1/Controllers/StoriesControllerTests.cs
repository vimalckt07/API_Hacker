using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NewsAPI.Controllers;
using NewsAPI.Model;
using NewsAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAPI.Controllers.Tests
{
    [TestClass]
    public class StoriesControllerTests
    {
        [TestMethod]
        public async Task Get_ReturnsOkResult_WithStories()
        {
            // Arrange
            var hackerNewsServiceMock = new Mock<IHackerNewsService>();
            var loggerMock = new Mock<ILogger<StoriesController>>();

            // Prepare a list of stories to be returned by the mocked service
            var stories = new List<Story>
            {
                new Story { id = 1, title = "Story 1", url = "http://example.com/1" },
                new Story { id = 2, title = "Story 2", url = "http://example.com/2" }
            };

            hackerNewsServiceMock.Setup(service => service.GetNewestStoriesAsync())
                                 .ReturnsAsync(stories);

            var controller = new StoriesController(hackerNewsServiceMock.Object, loggerMock.Object);

            // Act
            var result = await controller.Get();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value as List<Story>;
            Assert.IsNotNull(value);
            CollectionAssert.AreEqual(stories, value);
        }

        [TestMethod]
        public async Task Get_ReturnsInternalServerError_WhenExceptionThrown()
        {
            // Arrange
            var hackerNewsServiceMock = new Mock<IHackerNewsService>();
            var loggerMock = new Mock<ILogger<StoriesController>>();

            hackerNewsServiceMock.Setup(service => service.GetNewestStoriesAsync())
                                 .ThrowsAsync(new Exception("Test exception"));

            var controller = new StoriesController(hackerNewsServiceMock.Object, loggerMock.Object);

            // Act
            var result = await controller.Get();

            // Assert
            // Verify that the result is of type StatusCodeResult with status code 500
            var statusCodeResult = result as ObjectResult;
            Assert.IsNotNull(statusCodeResult);
            Assert.AreEqual(500, statusCodeResult.StatusCode);
            Assert.AreEqual("An error occurred while fetching the newest stories.", statusCodeResult.Value);

            // Verify that the logger logged an error
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while fetching the newest stories.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}