namespace Archiver.WebApp.Clients;

public class ApiGatewayClient(HttpClient httpClient)
{
    public HttpClient HttpClient { get; } = httpClient;
}
