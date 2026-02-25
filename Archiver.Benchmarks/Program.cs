using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Linq;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<WeatherForecastBenchmark>();
    }
}

[MemoryDiagnoser]
public class WeatherForecastBenchmark
{
    private readonly string[] summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    [Benchmark]
    public WeatherForecast[] Baseline()
    {
        // Matches the logic before optimization in Archiver.Services/Program.cs
        // Note: The original code had var now = DateTime.Now; outside loop.
        var now = DateTime.Now;
        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();
        return forecast;
    }

    [Benchmark]
    public WeatherForecast[] Optimized()
    {
        // Matches the optimized logic
        var today = DateOnly.FromDateTime(DateTime.Now);
        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                today.AddDays(index),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();
        return forecast;
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
