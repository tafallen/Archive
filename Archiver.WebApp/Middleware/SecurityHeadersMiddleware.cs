using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Archiver.WebApp.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next)
{
    private const int NonceLength = 32;
    private const int Base64NonceLength = 44; // (32 / 3 + 1) * 4 = 44
    private const string CspPrefix = "default-src 'self'; script-src 'self' 'nonce-";
    private const string CspSuffix = "' 'strict-dynamic'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self' ws: wss:; frame-ancestors 'self'";
    private static readonly int TotalCspLength = CspPrefix.Length + Base64NonceLength + CspSuffix.Length;

    public async Task InvokeAsync(HttpContext context)
    {
        Span<byte> bytes = stackalloc byte[NonceLength];
        RandomNumberGenerator.Fill(bytes);

        // Wrap the nonce in an object to defer Base64 string allocation.
        // This object stores the 32 bytes as primitive fields to avoid an additional byte[] heap allocation.
        var wrapper = new NonceWrapper(bytes);
        context.Items["csp-nonce"] = wrapper;

        // Create the CSP header in a single allocation.
        var cspHeader = string.Create(TotalCspLength, wrapper, (span, state) =>
        {
            CspPrefix.AsSpan().CopyTo(span);
            state.WriteBase64To(span.Slice(CspPrefix.Length, Base64NonceLength));
            CspSuffix.AsSpan().CopyTo(span.Slice(CspPrefix.Length + Base64NonceLength));
        });

        context.Response.Headers.Append("Content-Security-Policy", cspHeader);

        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        context.Response.Headers.Append("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
        context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");

        if (!context.Response.Headers.ContainsKey("Cache-Control"))
        {
            context.Response.Headers.Append("Cache-Control", "no-store, no-cache, max-age=0, must-revalidate");
            context.Response.Headers.Append("Pragma", "no-cache");
        }

        await next(context);
    }

    private sealed class NonceWrapper
    {
        // 32 bytes = 4 * 8-byte longs
        private readonly long _l1, _l2, _l3, _l4;
        private string? _cachedNonce;

        public NonceWrapper(ReadOnlySpan<byte> bytes)
        {
            _l1 = MemoryMarshal.Read<long>(bytes[0..8]);
            _l2 = MemoryMarshal.Read<long>(bytes[8..16]);
            _l3 = MemoryMarshal.Read<long>(bytes[16..24]);
            _l4 = MemoryMarshal.Read<long>(bytes[24..32]);
        }

        public void WriteBase64To(Span<char> destination)
        {
            Span<byte> bytes = stackalloc byte[NonceLength];
            MemoryMarshal.Write(bytes[0..8], ref Unsafe.AsRef(in _l1));
            MemoryMarshal.Write(bytes[8..16], ref Unsafe.AsRef(in _l2));
            MemoryMarshal.Write(bytes[16..24], ref Unsafe.AsRef(in _l3));
            MemoryMarshal.Write(bytes[24..32], ref Unsafe.AsRef(in _l4));
            Convert.TryToBase64Chars(bytes, destination, out _);
        }

        public override string ToString()
        {
            if (_cachedNonce != null) return _cachedNonce;

            Span<byte> bytes = stackalloc byte[NonceLength];
            MemoryMarshal.Write(bytes[0..8], ref Unsafe.AsRef(in _l1));
            MemoryMarshal.Write(bytes[8..16], ref Unsafe.AsRef(in _l2));
            MemoryMarshal.Write(bytes[16..24], ref Unsafe.AsRef(in _l3));
            MemoryMarshal.Write(bytes[24..32], ref Unsafe.AsRef(in _l4));
            return _cachedNonce = Convert.ToBase64String(bytes);
        }
    }
}
