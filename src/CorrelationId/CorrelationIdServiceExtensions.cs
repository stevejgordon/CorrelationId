using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CorrelationId
{
    public static class CorrelationIdServiceExtensions
    {
        public static void AddCorrelationId(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
            serviceCollection.TryAddTransient<ICorrelationContextFactory, CorrelationContextFactory>();
        }
    }
}
