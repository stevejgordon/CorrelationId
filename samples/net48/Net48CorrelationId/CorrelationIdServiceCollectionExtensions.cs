using Microsoft.Extensions.DependencyInjection;

namespace Net48CorrelationId
{
    public static class CorrelationIdServiceCollectionExtensions
    {
        public static void UseCorrelationIdMiddleware(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<CorrelationIdMiddlewareWrapper>();
        }
    }
}