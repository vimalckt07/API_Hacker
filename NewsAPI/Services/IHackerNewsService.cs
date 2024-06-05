using Microsoft.Extensions.Caching.Memory;
using NewsAPI.Model;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace NewsAPI.Services
{
    /// <summary>
    /// Service to fetch the newest stories from Hacker News API.
    /// </summary>
    public interface IHackerNewsService
    {
        Task<IEnumerable<Story>> GetNewestStoriesAsync();
    }
    /// <summary>
    /// Initializes a new instance of the HackerNewsService.
    /// </summary>
    /// <param name="cache">The memory cache.</param>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="logger">The logger.</param>
    public class HackerNewsService : IHackerNewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HackerNewsService> _logger;
        private const string CacheKey = "NewStories";
        private const string HackerNewsUrl = "https://hacker-news.firebaseio.com/v0/newstories.json";
        private const string StoryUrl = "https://hacker-news.firebaseio.com/v0/item/{0}.json";

        public HackerNewsService(IMemoryCache cache, HttpClient httpClient, ILogger<HackerNewsService> logger)
        {
            _cache = cache;
            _httpClient = httpClient;
            _logger = logger;
        }
        /// <summary>
        /// Fetches the newest stories from the Hacker News.
        /// </summary>
        /// <returns>A list of the newest stories are.</returns>        
        public async Task<IEnumerable<Story>> GetNewestStoriesAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out IEnumerable<Story> stories))
            {
                var top200Ids = await GetTop200IdsAsync();

                // Create tasks to fetch stories in parallel
                var storyTasks = top200Ids.Select(id => GetStoryAsync(id)).ToList();

                // Wait for all the tasks to complete
                var storyResults = await Task.WhenAll(storyTasks);

                // Filter out null stories
                stories = storyResults.Where(story => story != null).ToList();

                // Cache the stories
                _cache.Set(CacheKey, stories, TimeSpan.FromMinutes(10));
            }

            return stories;
        }

        private async Task<IEnumerable<int>> GetTop200IdsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(HackerNewsUrl);
                response.EnsureSuccessStatusCode();
                var ids = await response.Content.ReadFromJsonAsync<IEnumerable<int>>();
                return ids.Take(200);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching top 200 IDs: {ex.Message}");
                return Enumerable.Empty<int>();
            }
        }

        /// <summary>
        /// Fetches a story by its ID from Hacker News.
        /// </summary>
        /// <param name="id">The story ID.</param>
        /// <returns>The story, or null if an error occurred.</returns>
        private async Task<Story> GetStoryAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync(string.Format(StoryUrl, id));
                response.EnsureSuccessStatusCode();
                var story = await response.Content.ReadFromJsonAsync<Story>();
                return story;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching story {id}: {ex.Message}");
                return null;
            }
        }
    }
}
