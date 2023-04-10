using System.Threading.Tasks;
using CorrelationId;
using CorrelationId.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Net48CorrelationId
{
    public static class CorrelationIdServiceCollectionExtensions
    {
        public static void UseCorrelationIdMiddleware(this IServiceCollection serviceCollection, IHttpClientBuilder httpClientBuilder)
        {
            serviceCollection.UseCorrelationIdMiddleware();
            httpClientBuilder.AddHttpMessageHandler<CorrelationIdMiddlewareWrapper>();
        }

        public static void UseCorrelationIdMiddleware(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient(x =>
            {
                return new CorrelationIdMiddleware(
                    next =>
                    {
                        var correlationContextAccessor = x.GetService<ICorrelationContextAccessor>();
                        // copied from CorrelationId.HttpClient.CorrelationIdHandler
                        if (!string.IsNullOrEmpty(correlationContextAccessor?.CorrelationContext?.CorrelationId) &&
                            !next.Request.Headers.ContainsKey(correlationContextAccessor.CorrelationContext.Header))
                        {
                            next.Request.Headers.Add(correlationContextAccessor.CorrelationContext.Header,
                                correlationContextAccessor.CorrelationContext.CorrelationId);
                        }

                        return Task.CompletedTask;
                    },
                    x.GetService<ILogger<CorrelationIdMiddleware>>(),
                    x.GetService<IOptions<CorrelationIdOptions>>(),
                    x.GetService<ICorrelationIdProvider>());
            });
            
            serviceCollection.AddTransient<CorrelationIdMiddlewareWrapper>();
        }
    }
}