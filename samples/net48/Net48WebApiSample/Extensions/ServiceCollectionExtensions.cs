using Microsoft.Extensions.DependencyInjection;

namespace Net48WebApiSample.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddControllers(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<Controllers.CorrelationIdController>();
        }
    }
}