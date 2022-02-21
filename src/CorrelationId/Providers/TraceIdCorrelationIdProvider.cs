using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;

namespace CorrelationId.Providers;

/// <summary>
///     Sets the correlation ID to match the TraceIdentifier set on the <see cref="HttpContext" />.
/// </summary>
public class TraceIdCorrelationIdProvider : ICorrelationIdProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    public TraceIdCorrelationIdProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public string GenerateCorrelationId()
    {
        return _httpContextAccessor.HttpContext?.TraceIdentifier;
    }
}