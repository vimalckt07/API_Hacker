using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NewsAPI.Model;
using NewsAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace NewsAPITests1.Services
{
    [TestClass]
    public class HackerNewsServiceTests
    {
        private Mock<IMemoryCache> _cacheMock = new Mock<IMemoryCache>();
        private Mock<ILogger<HackerNewsService>> _loggerMock = new Mock<ILogger<HackerNewsService>>();
        private Mock<HttpMessageHandler> _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
                
        [TestMethod]
        public async Task GetNewestStoriesAsync_ReturnsStories_WhenApiCallSucceeds()
        {
            // Arrange
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            var hackerNewsService = new HackerNewsService(_cacheMock.Object, httpClient, _loggerMock.Object);

            // Prepare story IDs to be returned by the mocked API
            var storyIds = new[] { 40514312, 40514287, 40514282 };
            // Prepare the stories to be returned by the mocked API for each ID
            var stories = new List<Story>
            {
                new Story { id = 40514312, title = "The new Framework Laptop 13 with Intel Core Ultra Series 1 CPUs", url = "https://frame.work/ca/en/blog/introducing-the-new-framework-laptop-13-with-intel-core-ultra-series-1-processors" },
                new Story { id = 40514287, title = "Rack-Scale Security Attestation for the Oxide Cloud Computer", url = "https://www.youtube.com/watch?v=fEy0VKSFnO0" },
                new Story { id = 40514282, title = "Kino: Pro Video Camera", url = "https://www.lux.camera/introducing-kino-pro-video-camera/" }
            };

            // Setup cache mock for TryGetValue
            // Setup cache mock to simulate a cache miss
            object cacheEntry;
            _cacheMock.Setup(c => c.TryGetValue(It.IsAny<object>(), out cacheEntry)).Returns(false);

            // Setup cache mock for Set
            _cacheMock.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());

            // Setup HttpClient mock
            // Setup HttpClient mock to return each story when their respective endpoints are called
            SetupHttpMessageHandler("https://hacker-news.firebaseio.com/v0/newstories.json", storyIds);
            foreach (var story in stories)
            {
                SetupHttpMessageHandler($"https://hacker-news.firebaseio.com/v0/item/{story.id}.json", story);
            }

            // Act
            // Call the method under test
            var result = await hackerNewsService.GetNewestStoriesAsync();

            // Assert
            // Verify the results are as expected
            Assert.IsNotNull(result);
            // Check if the result is of type List<Story>
            var storiesResult = result as List<Story>;
            Assert.IsNotNull(storiesResult, "The result is not a list of stories");

            // Assert the count of stories in the result
            Assert.AreEqual(3, storiesResult.Count);

            
        }
        // Helper method to setup HttpMessageHandler for specific URLs and responses
        private void SetupHttpMessageHandler<T>(string url, T response)
        {
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == url),
                    ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(response)
                });
        }
       
    }
}
