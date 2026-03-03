using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Archiver.Benchmarks;

[MemoryDiagnoser]
public class GetBytesBenchmarks
{
    private string _apiKey = "my-super-secret-api-key-which-is-long";
    private StringValues _extractedApiKey = new StringValues("my-super-secret-api-key-which-is-long");

    private string? _cachedApiKey;
    private byte[]? _cachedExpectedKeyBytes;

    [Benchmark(Baseline = true)]
    public bool CurrentCode()
    {
        var providedKeyBytes = Encoding.UTF8.GetBytes(_extractedApiKey.ToString());
        var expectedKeyBytes = Encoding.UTF8.GetBytes(_apiKey);

        return CryptographicOperations.FixedTimeEquals(providedKeyBytes, expectedKeyBytes);
    }

    [Benchmark]
    public bool OptimizedCode()
    {
        // Safe access (though race is fine here as it only replaces the cache)
        var cachedKey = _cachedApiKey;
        var cachedBytes = _cachedExpectedKeyBytes;

        if (cachedKey != _apiKey || cachedBytes == null)
        {
            cachedKey = _apiKey;
            cachedBytes = Encoding.UTF8.GetBytes(_apiKey);
            _cachedExpectedKeyBytes = cachedBytes;
            _cachedApiKey = cachedKey;
        }

        var extractedStr = _extractedApiKey.ToString();
        int maxBytes = Encoding.UTF8.GetMaxByteCount(extractedStr.Length);

        if (maxBytes <= 256)
        {
            Span<byte> providedKeyBytes = stackalloc byte[maxBytes];
            int length = Encoding.UTF8.GetBytes(extractedStr, providedKeyBytes);
            return CryptographicOperations.FixedTimeEquals(providedKeyBytes[..length], cachedBytes);
        }
        else
        {
            var providedKeyBytes = Encoding.UTF8.GetBytes(extractedStr);
            return CryptographicOperations.FixedTimeEquals(providedKeyBytes, cachedBytes);
        }
    }
}
