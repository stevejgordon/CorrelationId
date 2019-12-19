using CorrelationId;

namespace MvcCorrelationIdSample
{
    public class ScopedClass
    {
        private readonly ICorrelationContextAccessor _correlationContext;

        public ScopedClass(ICorrelationContextAccessor correlationContext)
        {
            _correlationContext = correlationContext;
        }

        public string GetCorrelationFromScoped => _correlationContext.CorrelationContext.CorrelationId;
    }

    public class TransientClass
    {
        private readonly ICorrelationContextAccessor _correlationContext;

        public TransientClass(ICorrelationContextAccessor correlationContext)
        {
            _correlationContext = correlationContext;
        }

        public string GetCorrelationFromScoped => _correlationContext.CorrelationContext.CorrelationId;
    }

    public class SingletonClass
    {
        private readonly ICorrelationContextAccessor _correlationContext;

        public SingletonClass(ICorrelationContextAccessor correlationContext)
        {
            _correlationContext = correlationContext;
        }

        public string GetCorrelationFromScoped => _correlationContext.CorrelationContext.CorrelationId;
    }
}
