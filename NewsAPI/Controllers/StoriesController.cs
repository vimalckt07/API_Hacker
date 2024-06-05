using Microsoft.AspNetCore.Mvc;
using NewsAPI.Services;

namespace NewsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoriesController : ControllerBase
    {
        private readonly IHackerNewsService _hackerNewsService;
        private readonly ILogger<StoriesController> _logger;
        public StoriesController(IHackerNewsService hackerNewsService, ILogger<StoriesController> logger)
        {
            _hackerNewsService = hackerNewsService;
            _logger = logger;
        }
        /// <summary>
        /// Gets the newest stories from Hacker News.
        /// </summary>
        /// <returns>A list of the newest stories.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            
            try
            {
                var stories = await _hackerNewsService.GetNewestStoriesAsync();
                return Ok(stories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the newest stories.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching the newest stories.");
            }
        }
    }

}
