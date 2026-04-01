using Archiver.Services.Auth;
using Archiver.Shared;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ["text/plain", "text/css", "application/javascript", "text/javascript", "text/html", "application/xml", "text/xml", "application/json", "text/json", "application/wasm"];
});

builder.Services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, null);
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

app.UseResponseCompression();

app.UseAuthentication();
app.UseAuthorization();

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
            WeatherForecast.Summaries[Random.Shared.Next(WeatherForecast.Summaries.Length)]
        );
    }
    return forecast;
})
.WithName("GetWeatherForecast")
.RequireAuthorization();

app.Run();

public partial class Program { }
