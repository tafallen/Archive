using Archiver.WebApp.Clients;
using Archiver.WebApp.Components;
using Archiver.WebApp.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<ApiGatewayClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration[ConfigurationKeys.ApiGatewayUrl] ?? throw new InvalidOperationException($"{ConfigurationKeys.ApiGatewayUrl} not configured."));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(Constants.ErrorPath, createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
