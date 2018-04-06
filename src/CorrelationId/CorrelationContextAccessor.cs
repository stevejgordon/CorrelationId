using System.Threading;

namespace CorrelationId
{
    /// <inheritdoc />
    public class CorrelationContextAccessor : ICorrelationContextAccessor
    {
        private static AsyncLocal<ICorrelationContext> _correlationContext = new AsyncLocal<ICorrelationContext>();

        /// <inheritdoc />
        public ICorrelationContext CorrelationContext
        {
            get => _correlationContext.Value;
            set => _correlationContext.Value = value;
        }
    }
}