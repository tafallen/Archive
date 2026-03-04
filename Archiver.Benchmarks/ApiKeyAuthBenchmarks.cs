using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Concurrent;

namespace Archiver.Benchmarks;

[MemoryDiagnoser]
public class ApiKeyAuthBenchmarks
{
    private readonly string _schemeName = "ApiKey";
    private static readonly Claim[] CachedClaims = { new(ClaimTypes.Name, "ApiKeyUser") };
    private static readonly ConcurrentDictionary<string, AuthenticationTicket> CachedTickets = new();
    private static readonly ConcurrentDictionary<string, AuthenticateResult> CachedResults = new();

    [GlobalSetup]
    public void Setup()
    {
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(CachedClaims, _schemeName)), _schemeName);
        CachedTickets.TryAdd(_schemeName, ticket);
        CachedResults.TryAdd(_schemeName, AuthenticateResult.Success(ticket));
    }

    [Benchmark(Baseline = true)]
    public AuthenticateResult DictionaryCacheGetOrAdd()
    {
        var ticket = CachedTickets.GetOrAdd(_schemeName, schemeName =>
        {
            var identity = new ClaimsIdentity(CachedClaims, schemeName);
            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationTicket(principal, schemeName);
        });

        return AuthenticateResult.Success(ticket);
    }

    [Benchmark]
    public AuthenticateResult DictionaryCacheTryGetValue()
    {
        if (!CachedTickets.TryGetValue(_schemeName, out var ticket))
        {
            var identity = new ClaimsIdentity(CachedClaims, _schemeName);
            var principal = new ClaimsPrincipal(identity);
            ticket = new AuthenticationTicket(principal, _schemeName);
            CachedTickets.TryAdd(_schemeName, ticket);
        }

        return AuthenticateResult.Success(ticket);
    }

    [Benchmark]
    public AuthenticateResult DictionaryCacheAuthenticateResult()
    {
        if (!CachedResults.TryGetValue(_schemeName, out var result))
        {
            var identity = new ClaimsIdentity(CachedClaims, _schemeName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, _schemeName);
            result = AuthenticateResult.Success(ticket);
            CachedResults.TryAdd(_schemeName, result);
        }

        return result;
    }
}
