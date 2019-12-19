using CorrelationId.Abstractions;

namespace MvcSample
{
    public class ServiceWhichUsesCorrelationContext
    {
        private readonly ICorrelationContextAccessor _correlationContextAccessor;

        public ServiceWhichUsesCorrelationContext(ICorrelationContextAccessor correlationContextAccessor) => _correlationContextAccessor = correlationContextAccessor;

        public string DoStuff()
        {
            var correlationId = _correlationContextAccessor.CorrelationContext.CorrelationId;

            return $"Formatted correlation ID:{correlationId}";
        }
    }
}
