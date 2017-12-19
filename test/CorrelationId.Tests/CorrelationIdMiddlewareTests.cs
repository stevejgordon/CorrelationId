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
               .Configure(app => app.UseCorrelationId())
               .ConfigureServices(sc => sc.AddCorrelationId());

            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            var expectedHeaderName = new CorrelationIdOptions().Header;

            var header = response.Headers.GetValues(expectedHeaderName);

            Assert.NotNull(header);
        }

        [Fact]
        public async Task DoesNotThrowException_WhenOptionSetToTrue_IfHeaderIsAlreadySet()
        {
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseCorrelationId();
                    app.UseCorrelationId(); // header will already be set on this second use of the middleware
                })
                .ConfigureServices(sc => sc.AddCorrelationId());

            var server = new TestServer(builder);

            await server.CreateClient().GetAsync("");

            // Note: This test will only fail if the middleware is causing an exception by trying to set a response header which already exists.
        }

        [Fact]
        public async Task DoesNotReturnCorrelationIdInResponseHeader_WhenOptionSetToFalse()
        {
            var options = new CorrelationIdOptions { IncludeInResponse = false };

            var builder = new WebHostBuilder()
               .Configure(app => app.UseCorrelationId(options))
               .ConfigureServices(sc => sc.AddCorrelationId());

            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            var headerExists = response.Headers.TryGetValues(options.Header, out IEnumerable<string> _);

            Assert.False(headerExists);
        }

        [Fact]
        public async Task CorrelationIdHeaderFieldName_MatchesOptions()
        {
            const string customHeader = "X-Test-Header";

            var options = new CorrelationIdOptions { Header = customHeader };

            var builder = new WebHostBuilder()
               .Configure(app => app.UseCorrelationId(options))
               .ConfigureServices(sc => sc.AddCorrelationId());

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
               .Configure(app => app.UseCorrelationId(customHeader))
               .ConfigureServices(sc => sc.AddCorrelationId());

            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            var header = response.Headers.GetValues(customHeader);

            Assert.NotNull(header);
        }

        [Fact]
        public async Task CorrelationId_SetToCorrelationIdFromRequestHeader()
        {
            var expectedHeaderName = new CorrelationIdOptions().Header;
            var expectedHeaderValue = "123456";

            var builder = new WebHostBuilder()
               .Configure(app => app.UseCorrelationId())
               .ConfigureServices(sc => sc.AddCorrelationId());

            var server = new TestServer(builder);

            var request = new HttpRequestMessage();
            request.Headers.Add(expectedHeaderName, expectedHeaderValue);

            var response = await server.CreateClient().SendAsync(request);

            var header = response.Headers.GetValues(expectedHeaderName);

            Assert.Single(header, expectedHeaderValue);
        }
    }
}
