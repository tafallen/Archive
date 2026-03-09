using BenchmarkDotNet.Attributes;
using System;
using System.Security.Cryptography;

namespace Archiver.Benchmarks;

[MemoryDiagnoser]
public class SecurityHeadersBenchmark
{
    private const int NonceLength = 32;

    [Benchmark(Baseline = true)]
    public string CurrentCode()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(NonceLength));
    }

    [Benchmark]
    public string OptimizedCode()
    {
        Span<byte> bytes = stackalloc byte[NonceLength];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
