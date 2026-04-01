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

// Configuration validation
var configuration = app.Services.GetRequiredService<IConfiguration>();
var internalApiKey = configuration["ReverseProxy:Routes:0:Transforms:0:Set"];
if (string.IsNullOrEmpty(internalApiKey) || internalApiKey == "INTERNAL_API_KEY_REQUIRED")
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    if (app.Environment.IsDevelopment())
    {
        logger.LogWarning("X-Internal-Key is not properly configured. Using placeholder or empty value.");
    }
    else
    {
        logger.LogCritical("X-Internal-Key is NOT configured. Downstream services will reject requests.");
        // In a real production scenario, we might want to throw an exception here
        // throw new InvalidOperationException("X-Internal-Key must be configured in production.");
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
