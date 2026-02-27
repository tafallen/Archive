namespace Archiver.Shared;

public readonly record struct WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public const int MinTemperatureC = -20;
    public const int MaxTemperatureC = 55;
    public const int ForecastDays = 5;

    private const int FahrenheitFreezingPoint = 32;
    private const int ConversionNumerator = 9;
    private const int ConversionDenominator = 5;

    public int TemperatureF => FahrenheitFreezingPoint + (int)((long)TemperatureC * ConversionNumerator / ConversionDenominator);
}
