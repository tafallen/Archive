using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Linq;
using Archiver.Shared;

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

    [Benchmark(Baseline = true)]
    public WeatherForecastClass[] TaskDescriptionUnoptimized()
    {
        // This matches the "Current Code" description in the task
        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecastClass
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();
        return forecast;
    }

    [Benchmark]
    public WeatherForecast[] CurrentCode()
    {
        // Matches the current code in Archiver.Services/Program.cs
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

    [Benchmark]
    public WeatherForecast[] NoLinq()
    {
        // Proposed optimization removing LINQ
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
    }

    [Benchmark]
    public WeatherForecast[] StructOptimization()
    {
        // Proposed optimization using struct
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
    }
}

public record WeatherForecastClass(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
