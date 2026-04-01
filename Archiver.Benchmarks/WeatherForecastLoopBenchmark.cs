using BenchmarkDotNet.Attributes;
using Archiver.Shared;

namespace Archiver.Benchmarks;

[MemoryDiagnoser]
public class WeatherForecastLoopBenchmark
{
    private readonly string[] summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    [Benchmark(Baseline = true)]
    public WeatherForecast[] Baseline()
    {
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
    }

    [Benchmark]
    public WeatherForecast[] Optimized()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var startDayNumber = today.DayNumber;
        var forecast = new WeatherForecast[WeatherForecast.ForecastDays];
        int summariesLength = summaries.Length;
        for (int i = 0; i < WeatherForecast.ForecastDays; i++)
        {
            forecast[i] = new WeatherForecast
            (
                DateOnly.FromDayNumber(startDayNumber + i + 1),
                Random.Shared.Next(WeatherForecast.MinTemperatureC, WeatherForecast.MaxTemperatureC),
                summaries[Random.Shared.Next(summariesLength)]
            );
        }
        return forecast;
    }
}
