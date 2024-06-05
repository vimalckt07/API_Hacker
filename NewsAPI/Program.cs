using Microsoft.OpenApi.Models;
using NewsAPI.ExceptionHandling;
using NewsAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins("http://localhost:4200") // Adjust the origin to match your Angular application's URL
                   .AllowAnyHeader()
                   .AllowAnyMethod();
    });
});

// Adding in-memory caching service
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// Registering HttpClient for IHackerNewsService with a handler lifetime
builder.Services.AddHttpClient<IHackerNewsService, HackerNewsService>();
// Registering the HackerNewsService as a singleton service
builder.Services.AddSingleton<IHackerNewsService, HackerNewsService>();

builder.Services.AddEndpointsApiExplorer();
// Configuring Swagger for API documentation
builder.Services.AddSwaggerGen(options =>
{
    // Configure Swagger to include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

});
// Adding logging services
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); //// Enable Swagger in development mode
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hacker News API V1");
    });
}

// Use the CORS policy
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Using custom middleware for exception handling
app.UseMiddleware<ExceptionMiddleware>();

// Running the application
app.Run();

