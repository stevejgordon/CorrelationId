using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using CorrelationId.DependencyInjection;
using CorrelationId.HttpClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Net48CorrelationId;
using Net48WebApiSample.Extensions;

namespace Net48WebApiSample
{
    public static class DependencyInjection
    {
        public static void Register(HttpConfiguration config)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions
            {
                // Prefer to keep validation on at all times
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            GlobalConfiguration.Configuration.Services.Replace(
                typeof(IHttpControllerActivator),
                new MsDiHttpControllerActivator(serviceProvider));
            
            var correlationIdMiddlewareWrapper = serviceProvider.GetService<CorrelationIdMiddlewareWrapper>();
            config.MessageHandlers.Add(correlationIdMiddlewareWrapper);
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<NoOpDelegatingHandler>();

            serviceCollection.AddHttpClient("MyClient")
                .AddCorrelationIdForwarding() // add the handler to attach the correlation ID to outgoing requests for this named client
                .AddHttpMessageHandler<NoOpDelegatingHandler>();
            
            serviceCollection.AddDefaultCorrelationId(options =>
            {
                options.AddToLoggingScope = true;
                options.IgnoreRequestHeader = false;
                options.EnforceHeader = false;
                options.IncludeInResponse = true;
                options.RequestHeader = "X-Correlation-Id";
                options.ResponseHeader = "X-Correlation-Id";
                options.UpdateTraceIdentifier = false;
            });

            serviceCollection.UseCorrelationIdMiddleware();

            serviceCollection.AddControllers();
            
            serviceCollection.AddLogging(loggerFactory =>
            {
                loggerFactory.AddConsole();
                loggerFactory.SetMinimumLevel(LogLevel.Debug);
            });
        }
    }
    
    public class MsDiHttpControllerActivator : IHttpControllerActivator
    {
        private readonly ServiceProvider _provider;

        public MsDiHttpControllerActivator(ServiceProvider provider)
        {
            _provider = provider;
        }

        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor descriptor, Type type)
        {
            var scope = _provider.CreateScope();
            request.RegisterForDispose(scope);
            return (IHttpController)scope.ServiceProvider.GetRequiredService(type);
        }
    }
}