using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;

namespace Archiver.Benchmarks;

[MemoryDiagnoser]
public class CspHeaderBenchmarks
{
    private string nonce = "12345678901234567890123456789012345678901234";

    [Benchmark(Baseline = true)]
    public string Interpolation()
    {
        return $"default-src 'self'; script-src 'self' 'nonce-{nonce}' 'strict-dynamic'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self' ws: wss:; frame-ancestors 'self'";
    }

    [Benchmark]
    public string StringConcat()
    {
        return string.Concat("default-src 'self'; script-src 'self' 'nonce-", nonce, "' 'strict-dynamic'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self' ws: wss:; frame-ancestors 'self'");
    }
}
