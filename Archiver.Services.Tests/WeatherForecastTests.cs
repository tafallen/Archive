using Archiver.Services;
using Xunit;

namespace Archiver.Services.Tests;

public class WeatherForecastTests
{
    [Theory]
    [InlineData(0, 32)]      // Freezing point
    [InlineData(100, 212)]   // Boiling point
    [InlineData(-40, -40)]   // Parity point
    [InlineData(25, 77)]     // Standard warm day
    public void TemperatureF_CalculatesCorrectly(int tempC, int expectedTempF)
    {
        // Arrange
        var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Now), tempC, "Test");

        // Act
        var actualTempF = forecast.TemperatureF;

        // Assert
        Assert.Equal(expectedTempF, actualTempF);
    }
}
