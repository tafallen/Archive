namespace Archiver.Shared;

public readonly record struct WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    private const int FahrenheitFreezingPoint = 32;
    private const double CelsiusToFahrenheitMultiplier = 1.8;

    public int TemperatureF => FahrenheitFreezingPoint + (int)(TemperatureC * CelsiusToFahrenheitMultiplier);
}
