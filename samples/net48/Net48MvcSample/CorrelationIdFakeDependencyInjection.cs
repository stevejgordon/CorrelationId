using System.Web.Http;
using CorrelationId;
using CorrelationId.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Net48CorrelationId;

namespace Net48MvcSample
{
    public static class CorrelationIdFakeDependencyInjection
    {
        public static void Register(HttpConfiguration config)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDefaultCorrelationId(options =>
            {
                options.AddToLoggingScope = true;
                options.IgnoreRequestHeader = false;
                options.IncludeInResponse = true;
                options.RequestHeader = "X-Correlation-Id";
                options.ResponseHeader = "X-Correlation-Id";
                options.UpdateTraceIdentifier = false;
            });

            serviceCollection.UseCorrelationIdMiddleware();

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            serviceCollection.AddSingleton(_ => loggerFactory.CreateLogger<CorrelationIdMiddleware>());
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var correlationIdMiddlewareWrapper = serviceProvider.GetService<CorrelationIdMiddlewareWrapper>();
            
            config.MessageHandlers.Add(correlationIdMiddlewareWrapper);
        }
    }
}