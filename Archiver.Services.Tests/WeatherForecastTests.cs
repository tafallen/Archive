using Archiver.Shared;
using Xunit;

namespace Archiver.Services.Tests;

public class WeatherForecastTests
{
    [Theory]
    [InlineData(0, 32)]       // Freezing point
    [InlineData(100, 212)]    // Boiling point
    [InlineData(-40, -40)]    // Parity point
    [InlineData(25, 77)]      // Standard warm day
    [InlineData(1, 33)]       // Just above freezing: 32 + 1.8 -> 33
    [InlineData(-1, 31)]      // Just below freezing: 32 - 1.8 -> 31
    [InlineData(37, 98)]      // Body temperature: 32 + 66.6 -> 98 (truncation)
    [InlineData(-10, 14)]     // Cold day: 32 - 18 -> 14
    [InlineData(26, 78)]      // Truncation check: 26 * 1.8 = 46.8 -> 32 + 46 = 78
    [InlineData(10, 50)]      // Precision check: 10 * 1.8 = 18.0 -> 32 + 18 = 50. (10 / 0.5556 would be ~49)
    [InlineData(-100, -148)]  // Cold boundary: 32 - 180 = -148
    [InlineData(50, 122)]     // Hot boundary: 32 + 90 = 122
    public void TemperatureF_CalculatesCorrectly(int tempC, int expectedTempF)
    {
        // Arrange
        var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Now), tempC, "Test");

        // Act
        var actualTempF = forecast.TemperatureF;

        // Assert
        Assert.Equal(expectedTempF, actualTempF);
    }

    [Theory]
    [InlineData(300_000_000, 540_000_032)] // Valid int result, but overflows intermediate int calculation (300M * 9 > 2.14B)
    [InlineData(-300_000_000, -539_999_968)] // Negative valid int result, overflows intermediate
    public void TemperatureF_HandlesLargeValidInputs_WithoutIntermediateOverflow(int tempC, int expectedTempF)
    {
        var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Now), tempC, "Test");
        Assert.Equal(expectedTempF, forecast.TemperatureF);
    }

    [Fact]
    public void TemperatureF_MatchesFloatingPointCalculation_ForStandardRange()
    {
        for (int c = -100; c <= 100; c++)
        {
            var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Now), c, "Test");

            // Expected logic: truncate towards zero for division
            // c * 9 / 5 is integer division.
            // (int)(c * 1.8) also truncates towards zero.
            int expected = 32 + (int)(c * 1.8);

            Assert.Equal(expected, forecast.TemperatureF);
        }
    }
}
