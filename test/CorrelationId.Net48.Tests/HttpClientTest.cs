using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CorrelationId.Net48.Tests
{
    public class HttpClientTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public HttpClientTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }
        
        [Fact]
        public async Task CallApi_ShouldReturnCorrelationId()
        {
            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri("http://localhost:31488");

            var response = await client.GetAsync("/api/CorrelationId");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            Assert.Single(response.Headers.GetValues("X-Correlation-Id"));
            Assert.True(Guid.TryParse(response.Headers.GetValues("X-Correlation-Id").Single(), out _));

            _testOutputHelper.WriteLine("CorrelationId returned in header: " +
                                        response.Headers.GetValues("X-Correlation-Id").Single());
        }
    }
}