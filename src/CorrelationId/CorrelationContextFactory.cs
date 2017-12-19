namespace CorrelationId
{
    public class CorrelationContextFactory : ICorrelationContextFactory
    {
        private readonly ICorrelationContextAccessor _correlationContextAccessor;
        
        public CorrelationContextFactory(ICorrelationContextAccessor correlationContextAccessor)
        {
            _correlationContextAccessor = correlationContextAccessor;
        }

        public CorrelationContext Create(string correlationId)
        {
            var correlationContext = new CorrelationContext(correlationId);

            if (_correlationContextAccessor != null)
            {
                _correlationContextAccessor.CorrelationContext = correlationContext;
            }

            return correlationContext;
        }

        public void Dispose()
        {
            if (_correlationContextAccessor != null)
            {
                _correlationContextAccessor.CorrelationContext = null;
            }
        }
    }
}