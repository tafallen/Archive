using BenchmarkDotNet.Attributes;
using System;
using System.Security.Cryptography;
using System.Buffers.Text;

namespace Archiver.Benchmarks;

[MemoryDiagnoser]
public class SecurityHeadersBenchmark
{
    private const int NonceLength = 32;
    private const string CspPrefix = "default-src 'self'; script-src 'self' 'nonce-";
    private const string CspSuffix = "' 'strict-dynamic'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self' ws: wss:; frame-ancestors 'self'";

    [Benchmark(Baseline = true)]
    public string CurrentCode()
    {
        Span<byte> bytes = stackalloc byte[NonceLength];
        RandomNumberGenerator.Fill(bytes);
        var nonce = Convert.ToBase64String(bytes);
        return string.Concat(CspPrefix, nonce, CspSuffix);
    }

    [Benchmark]
    public string OptimizedCode()
    {
        Span<byte> bytes = stackalloc byte[NonceLength];
        RandomNumberGenerator.Fill(bytes);

        // 32 bytes Base64 length is 44.
        // CspPrefix is 45.
        // CspSuffix is 127.
        // Total is 216.
        return string.Create(216, bytes, (span, b) =>
        {
            CspPrefix.AsSpan().CopyTo(span);
            Convert.TryToBase64Chars(b, span.Slice(CspPrefix.Length), out int written);
            CspSuffix.AsSpan().CopyTo(span.Slice(CspPrefix.Length + written));
        });
    }
}
