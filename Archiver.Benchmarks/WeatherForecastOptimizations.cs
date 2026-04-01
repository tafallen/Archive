using BenchmarkDotNet.Attributes;
using Archiver.Shared;

namespace Archiver.Benchmarks;

[MemoryDiagnoser]
public class WeatherForecastOptimizations
{
    [Benchmark(Baseline = true)]
    public WeatherForecast[] UnoptimizedDateTime()
    {
        var forecast = new WeatherForecast[5];
        for (int i = 0; i < 5; i++)
        {
            forecast[i] = new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(i + 1)),
                Random.Shared.Next(-20, 55),
                WeatherForecast.Summaries[Random.Shared.Next(WeatherForecast.Summaries.Length)]
            );
        }
        return forecast;
    }

    [Benchmark]
    public WeatherForecast[] OptimizedDateTime()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var forecast = new WeatherForecast[5];
        for (int i = 0; i < 5; i++)
        {
            forecast[i] = new WeatherForecast
            (
                today.AddDays(i + 1),
                Random.Shared.Next(-20, 55),
                WeatherForecast.Summaries[Random.Shared.Next(WeatherForecast.Summaries.Length)]
            );
        }
        return forecast;
    }

    [Benchmark]
    public int UnoptimizedMath()
    {
        int sum = 0;
        for(int i=-20; i<55; i++)
        {
             sum += 32 + (int)(i * 1.8);
        }
        return sum;
    }

    [Benchmark]
    public int OptimizedMath()
    {
        int sum = 0;
        for(int i=-20; i<55; i++)
        {
             sum += 32 + (i * 9 / 5);
        }
        return sum;
    }
}
