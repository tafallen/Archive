using Archiver.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var today = DateOnly.FromDateTime(DateTime.Now);
    var forecast = new WeatherForecast[5];
    for (int i = 0; i < 5; i++)
    {
        forecast[i] = new WeatherForecast
        (
            today.AddDays(i + 1),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        );
    }
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();
