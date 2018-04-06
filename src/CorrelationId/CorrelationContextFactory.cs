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
        public ICorrelationContext Create(string correlationId, string header)
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