using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Linq;
using Archiver.Shared;
using Archiver.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
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
