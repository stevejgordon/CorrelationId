using CorrelationId.Abstractions;

namespace CorrelationId;

/// <inheritdoc />
public class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<CorrelationContext> _correlationContext = new();

    /// <inheritdoc />
    public CorrelationContext CorrelationContext
    {
        get => _correlationContext.Value;
        set => _correlationContext.Value = value;
    }
}