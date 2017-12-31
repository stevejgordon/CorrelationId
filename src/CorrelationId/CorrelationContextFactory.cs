namespace CorrelationId
{
    /// <inheritdoc />
    public class CorrelationContextFactory : ICorrelationContextFactory
    {
        private readonly ICorrelationContextAccessor _correlationContextAccessor;
        
        public CorrelationContextFactory(ICorrelationContextAccessor correlationContextAccessor)
        {
            _correlationContextAccessor = correlationContextAccessor;
        }

        /// <inheritdoc />
        public CorrelationContext Create(string correlationId)
        {
            var correlationContext = new CorrelationContext(correlationId);

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