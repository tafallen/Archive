using Archiver.Shared;

namespace Archiver.WebApp.Clients;

public class WeatherApiClient(HttpClient httpClient)
{
    public async Task<WeatherForecast[]> GetWeatherForecastAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast", cancellationToken) ?? [];
    }
}
