using CorrelationId.Abstractions;

namespace CorrelationId.Providers;

/// <summary>
///     Generates a correlation ID using a new GUID.
/// </summary>
public class GuidCorrelationIdProvider : ICorrelationIdProvider
{
    /// <inheritdoc />
    public string GenerateCorrelationId()
    {
        return Guid.NewGuid().ToString();
    }
}