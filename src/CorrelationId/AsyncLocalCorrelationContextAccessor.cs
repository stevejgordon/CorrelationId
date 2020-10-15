using System.Threading;
using CorrelationId.Abstractions;

namespace CorrelationId
{
    /// <inheritdoc />
    internal sealed class AsyncLocalCorrelationContextAccessor : IMutableCorrelationContextAccessor
    {
        private static readonly AsyncLocal<CorrelationContext> _correlationContext = new AsyncLocal<CorrelationContext>();

        /// <inheritdoc cref="IMutableCorrelationContextAccessor.CorrelationContext" />
        public CorrelationContext CorrelationContext
        {
            get => _correlationContext.Value;
            set => _correlationContext.Value = value;
        }
    }
}