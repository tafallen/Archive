using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Standard handling for TooManyRequests
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
    };
});

var app = builder.Build();

// Validate internal API key configuration
var internalApiKey = builder.Configuration["ReverseProxy:Routes:0:Transforms:0:Set"];
if (string.IsNullOrEmpty(internalApiKey) || internalApiKey == "INTERNAL_API_KEY_REQUIRED")
{
    var message = "Internal API Key is not configured correctly. 'X-Internal-Key' transform is missing, empty, or set to placeholder.";
    if (app.Environment.IsDevelopment())
    {
        app.Logger.LogWarning(message);
    }
    else
    {
        app.Logger.LogCritical(message);
        throw new InvalidOperationException(message);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.MapReverseProxy();

app.Run();

public partial class Program { }
