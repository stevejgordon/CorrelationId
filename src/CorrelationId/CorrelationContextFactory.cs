using CorrelationId.Abstractions;

namespace CorrelationId
{
    /// <inheritdoc />
    internal sealed class CorrelationContextFactory : ICorrelationContextFactory
    {
        private readonly IMutableCorrelationContextAccessor _correlationContextAccessor;

        /// <summary>
        /// Initialises a new instance of <see cref="CorrelationContextFactory" />.
        /// </summary>
        public CorrelationContextFactory() 
            : this(null)
        { }

        /// <summary>
        /// Initialises a new instance of the <see cref="CorrelationContextFactory"/> class.
        /// </summary>
        /// <param name="correlationContextAccessor">The <see cref="ICorrelationContextAccessor"/> through which the <see cref="CorrelationContext"/> will be set.</param>
        public CorrelationContextFactory(IMutableCorrelationContextAccessor correlationContextAccessor)
        {
            _correlationContextAccessor = correlationContextAccessor;
        }

        /// <inheritdoc />
        public CorrelationContext Create(string correlationId, string header)
        {
            var correlationContext = new CorrelationContext(correlationId, header);

            if (_correlationContextAccessor != null)
            {
                _correlationContextAccessor.CorrelationContext = correlationContext;
            }

            return correlationContext;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_correlationContextAccessor != null)
            {
                _correlationContextAccessor.CorrelationContext = null;
            }
        }
    }
}