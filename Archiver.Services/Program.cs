using Archiver.Services.Auth;
using Archiver.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", null);
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    // DateTime.Now captured outside loop for performance (verified in benchmarks)
    // Using DateOnly.DayNumber to avoid repeated DateOnly struct creation and validation overhead
    var today = DateOnly.FromDateTime(DateTime.Now);
    var startDayNumber = today.DayNumber;
    var forecast = new WeatherForecast[WeatherForecast.ForecastDays];
    for (int i = 0; i < WeatherForecast.ForecastDays; i++)
    {
        forecast[i] = new WeatherForecast
        (
            DateOnly.FromDayNumber(startDayNumber + i + 1),
            Random.Shared.Next(WeatherForecast.MinTemperatureC, WeatherForecast.MaxTemperatureC),
            summaries[Random.Shared.Next(summaries.Length)]
        );
    }
    return forecast;
})
.WithName("GetWeatherForecast")
.RequireAuthorization();

app.Run();
