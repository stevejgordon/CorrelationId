using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CorrelationId.Tests
{
    public class CorrelationIdMiddlewareTests
    {
        [Fact]
        public async Task ReturnsCorrelationIdInResponseHeader_WhenOptionSetToTrue()
        {
            var builder = new WebHostBuilder()
               .Configure(app => app.UseCorrelationId());

            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            var expectedHeaderName = new CorrelationIdOptions().Header;

            var header = response.Headers.GetValues(expectedHeaderName);

            Assert.NotNull(header);            
        }

        [Fact]
        public async Task DoesNotReturnCorrelationIdInResponseHeader_WhenOptionSetToFalse()
        {
            var options = new CorrelationIdOptions { IncludeInResponse = false };

            var builder = new WebHostBuilder()
               .Configure(app => app.UseCorrelationId(options));

            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            var headerExists = response.Headers.TryGetValues(options.Header, out IEnumerable<string> headerValue);

            Assert.False(headerExists);
        }

        [Fact]
        public async Task CorrelationIdHeaderFieldName_MatchesOptions()
        {
            const string customHeader = "X-Test-Header";

            var options = new CorrelationIdOptions { Header = customHeader };

            var builder = new WebHostBuilder()
               .Configure(app => app.UseCorrelationId(options));

            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            var header = response.Headers.GetValues(customHeader);

            Assert.NotNull(header);
        }

        [Fact]
        public async Task CorrelationIdHeaderFieldName_MatchesStringOverload()
        {
            const string customHeader = "X-Test-Header";

            var builder = new WebHostBuilder()
               .Configure(app => app.UseCorrelationId(customHeader));

            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            var header = response.Headers.GetValues(customHeader);

            Assert.NotNull(header);
        }

        [Fact]
        public async Task CorrelationId_SetToCorrelationIdFromRequestHeader()
        {
            var expectedHeaderName = new CorrelationIdOptions().Header;

            var builder = new WebHostBuilder()
               .Configure(app => app.UseCorrelationId());

            var server = new TestServer(builder);

            var request = new HttpRequestMessage();
            request.Headers.Add(expectedHeaderName, "123456");

            var response = await server.CreateClient().SendAsync(request);
                        
            var header = response.Headers.GetValues(expectedHeaderName);

            Assert.NotNull(header);
        }
    }
}
