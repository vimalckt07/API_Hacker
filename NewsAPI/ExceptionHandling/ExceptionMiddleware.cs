using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace NewsAPI.ExceptionHandling
{
    
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Middleware to handle exceptions in the request pipeline.
        /// </summary>
        /// <param name="httpContext">The current HTTP context.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Call the next middleware in the pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log the exception and handle the response
                _logger.LogError(ex, "An unexpected error occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        // <summary>
        /// Handles the exception by setting the response status code and message.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="exception">The exception that occurred.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        // Extension method used to add the middleware to the HTTP request pipeline.
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error",
                Detailed = exception.Message //// Include detailed error message (in production)
            };
            // Serialize the response to JSON and write to the response body
            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
