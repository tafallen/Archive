namespace Archiver.Shared;

public readonly record struct WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    private const int FahrenheitFreezingPoint = 32;

    public int TemperatureF => FahrenheitFreezingPoint + (int)((long)TemperatureC * 9 / 5);
}
