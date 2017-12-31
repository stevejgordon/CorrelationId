using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CorrelationId
{
    public static class CorrelationIdServiceExtensions
    {
        /// <summary>
        /// Adds required services to support the Correlation ID functionality.
        /// </summary>
        /// <param name="serviceCollection"></param>
        public static void AddCorrelationId(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
            serviceCollection.TryAddTransient<ICorrelationContextFactory, CorrelationContextFactory>();
        }
    }
}
