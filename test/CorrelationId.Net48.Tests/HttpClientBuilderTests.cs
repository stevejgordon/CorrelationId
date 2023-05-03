using System;
using System.Linq;
using System.Threading.Tasks;
using CorrelationId.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Net48CorrelationId;
using Xunit;
using Xunit.Abstractions;

namespace CorrelationId.Net48.Tests
{
    public class HttpClientBuilderTests
    {
        private readonly Net48MvcSampleApiClient _net48MvcSampleApiClient;

        public HttpClientBuilderTests(ITestOutputHelper testOutputHelper)
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
            
            var httpClientBuilder = serviceCollection.AddHttpClient<Net48MvcSampleApiClient>();

            serviceCollection.UseCorrelationIdMiddleware(httpClientBuilder);
            
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddXUnit(testOutputHelper);
                builder.SetMinimumLevel(LogLevel.Debug);
            });
            serviceCollection.AddSingleton(_ => loggerFactory.CreateLogger<CorrelationIdMiddleware>());
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            _net48MvcSampleApiClient = serviceProvider.GetService<Net48MvcSampleApiClient>();
        }
        
        [Fact]
        public async Task CallTo_Net48Service_ShouldSetCorrelationId()
        {
            var response = await _net48MvcSampleApiClient.GetAsync();
            var responsePayload = await response.Content.ReadAsStringAsync();

            Assert.True(response.IsSuccessStatusCode, responsePayload);
            
            Assert.True(Guid.TryParse(responsePayload.Replace("\"", string.Empty), out _));
            
            Assert.Single(response.Headers.GetValues("X-Correlation-Id"));
            Assert.Equal(response.Headers.GetValues("X-Correlation-Id").Single(), responsePayload.Replace("\"", string.Empty));
        }
        
        [Fact]
        public async Task CallTo_Net48Service_MultipleCalls_ShouldSetDifferentCorrelationId()
        {
            var responseTask = _net48MvcSampleApiClient.GetAsync();
            var response2Task = _net48MvcSampleApiClient.GetAsync();
            Task.WaitAll(responseTask, response2Task);
            
            var response = await responseTask;
            var responsePayload = await response.Content.ReadAsStringAsync();
            Assert.True(response.IsSuccessStatusCode, responsePayload);
            
            var response2 = await response2Task;
            var response2Payload = await response2.Content.ReadAsStringAsync();

            Assert.True(response2.IsSuccessStatusCode, response2Payload);

            Assert.NotNull(response.Content);
            Assert.True(Guid.TryParse(responsePayload.Replace("\"", string.Empty), out _));
            
            Assert.NotNull(response2.Content);
            Assert.True(Guid.TryParse(response2Payload.Replace("\"", string.Empty), out _));
            
            Assert.NotEqual(response, response2);
        }
    }
}