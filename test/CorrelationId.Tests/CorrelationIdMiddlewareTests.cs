using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using System;

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
        public async Task DoesNotReturnCorrelationIdInResponseHeader_WhenIncludeInResponseIsFalse()
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
        public async Task CorrelationIdHeaderFieldName_MatchesHeaderOption()
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
        public async Task CorrelationIdHeaderFieldName_MatchesHeaderFromStringOverload()
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
            const string expectedHeaderValue = "123456";

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

        [Fact]
        public async Task CorrelationId_SetToGuid_WhenUseGuidForCorrelationId_IsTrue()
        {
            var options = new CorrelationIdOptions { UseGuidForCorrelationId = true };

            var builder = new WebHostBuilder()
                .Configure(app => app.UseCorrelationId(options))
                .ConfigureServices(sc => sc.AddCorrelationId());

            var server = new TestServer(builder);
            
            var response = await server.CreateClient().GetAsync("");

            var header = response.Headers.GetValues(new CorrelationIdOptions().Header);

            var isGuid = Guid.TryParse(header.FirstOrDefault(), out _);

            Assert.True(isGuid);
        }

        [Fact]
        public async Task CorrelationId_NotSetToGuid_WhenUseGuidForCorrelationId_IsFalse()
        {
            var options = new CorrelationIdOptions { UseGuidForCorrelationId = false };

            var builder = new WebHostBuilder()
                .Configure(app => app.UseCorrelationId(options))
                .ConfigureServices(sc => sc.AddCorrelationId());

            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            var header = response.Headers.GetValues(new CorrelationIdOptions().Header);

            var isGuid = Guid.TryParse(header.FirstOrDefault(), out _);

            Assert.False(isGuid);
        }

        [Fact]
        public async Task CorrelationId_ReturnedCorrectlyFromSingletonService()
        {
            var expectedHeaderName = new CorrelationIdOptions().Header;

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseCorrelationId();
                    app.Run(async rd =>
                    {
                        var singleton = app.ApplicationServices.GetService(typeof(SingletonClass)) as SingletonClass;
                        var scoped = app.ApplicationServices.GetService(typeof(ScopedClass)) as ScopedClass;

                        var data = Encoding.UTF8.GetBytes(singleton?.GetCorrelationFromScoped + "|" + scoped?.GetCorrelationFromScoped);

                        await rd.Response.Body.WriteAsync(data, 0, data.Length);
                    });
                })
                .ConfigureServices(sc =>
                {
                    sc.AddCorrelationId();
                    sc.TryAddSingleton<SingletonClass>();
                    sc.TryAddScoped<ScopedClass>();
                });

            var server = new TestServer(builder);

            // compare that first request matches the header and the scoped value
            var request = new HttpRequestMessage();

            var response = await server.CreateClient().SendAsync(request);

            var header = response.Headers.GetValues(expectedHeaderName).FirstOrDefault();

            var content = await response.Content.ReadAsStringAsync();
            var splitContent = content.Split('|');

            Assert.Equal(header, splitContent[0]);
            Assert.Equal(splitContent[0], splitContent[1]);

            // compare that second request matches the header and the scoped value
            var request2 = new HttpRequestMessage();

            var response2 = await server.CreateClient().SendAsync(request2);

            var header2 = response2.Headers.GetValues(expectedHeaderName).FirstOrDefault();

            var content2 = await response2.Content.ReadAsStringAsync();
            var splitContent2 = content2.Split('|');

            Assert.Equal(header2, splitContent2[0]);
            Assert.Equal(splitContent2[0], splitContent2[1]);
        }

        [Fact]
        public async Task CorrelationId_ReturnedCorrectlyFromTransientService()
        {
            var expectedHeaderName = new CorrelationIdOptions().Header;

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseCorrelationId();
                    app.Run(async rd =>
                    {
                        var transient = app.ApplicationServices.GetService(typeof(TransientClass)) as TransientClass;
                        var scoped = app.ApplicationServices.GetService(typeof(ScopedClass)) as ScopedClass;

                        var data = Encoding.UTF8.GetBytes(transient?.GetCorrelationFromScoped + "|" + scoped?.GetCorrelationFromScoped);

                        await rd.Response.Body.WriteAsync(data, 0, data.Length);
                    });
                })
                .ConfigureServices(sc =>
                {
                    sc.AddCorrelationId();
                    sc.TryAddTransient<TransientClass>();
                    sc.TryAddScoped<ScopedClass>();
                });

            var server = new TestServer(builder);

            // compare that first request matches the header and the scoped value
            var request = new HttpRequestMessage();

            var response = await server.CreateClient().SendAsync(request);

            var header = response.Headers.GetValues(expectedHeaderName).FirstOrDefault();

            var content = await response.Content.ReadAsStringAsync();
            var splitContent = content.Split('|');

            Assert.Equal(header, splitContent[0]);
            Assert.Equal(splitContent[0], splitContent[1]);

            // compare that second request matches the header and the scoped value
            var request2 = new HttpRequestMessage();

            var response2 = await server.CreateClient().SendAsync(request2);

            var header2 = response2.Headers.GetValues(expectedHeaderName).FirstOrDefault();

            var content2 = await response2.Content.ReadAsStringAsync();
            var splitContent2 = content2.Split('|');

            Assert.Equal(header2, splitContent2[0]);
            Assert.Equal(splitContent2[0], splitContent2[1]);
        }

        [Fact]
        public async Task CorrelationContextIncludesHeaderValue_WhichMatchesTheOriginalOptionsValue()
        {
            var options = new CorrelationIdOptions { Header = "custom-header" };

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseCorrelationId(options);

                    app.Use(async (ctx, next) =>
                    {
                        var accessor = ctx.RequestServices.GetService<ICorrelationContextAccessor>();
                        await ctx.Response.WriteAsync(accessor.CorrelationContext.Header);
                        await next();
                    });
                })
                .ConfigureServices(sc => sc.AddCorrelationId());

            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(body, options.Header);
        }

        [Fact]
        public async Task TraceIdentifier_IsNotUpdated_WhenUpdateTraceIdentifierIsFalse()
        {
            var options = new CorrelationIdOptions { UpdateTraceIdentifier = false };

            var expectedHeaderName = new CorrelationIdOptions().Header;
            const string expectedHeaderValue = "123456";
            
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseCorrelationId(options);

                    app.Use(async (ctx, next) =>
                    {
                        await ctx.Response.WriteAsync(ctx.TraceIdentifier);
                        await next();
                    });
                })
                .ConfigureServices(sc => sc.AddCorrelationId());

            var server = new TestServer(builder);

            var request = new HttpRequestMessage();
            request.Headers.Add(expectedHeaderName, expectedHeaderValue);

            var response = await server.CreateClient().SendAsync(request);
            
            var body = await response.Content.ReadAsStringAsync();

            Assert.NotEqual(body, expectedHeaderValue);
        }

        [Fact]
        public async Task TraceIdentifier_IsNotUpdated_WhenUpdateTraceIdentifierIsTrue()
        {
            var options = new CorrelationIdOptions { UpdateTraceIdentifier = true };

            var expectedHeaderName = new CorrelationIdOptions().Header;
            const string expectedHeaderValue = "123456";

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseCorrelationId(options);

                    app.Use(async (ctx, next) =>
                    {
                        await ctx.Response.WriteAsync(ctx.TraceIdentifier);
                        await next();
                    });
                })
                .ConfigureServices(sc => sc.AddCorrelationId());

            var server = new TestServer(builder);

            var request = new HttpRequestMessage();
            request.Headers.Add(expectedHeaderName, expectedHeaderValue);

            var response = await server.CreateClient().SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(body, expectedHeaderValue);
        }

        private class SingletonClass
        {
            private readonly ICorrelationContextAccessor _correlationContext;

            public SingletonClass(ICorrelationContextAccessor correlationContext)
            {
                _correlationContext = correlationContext;
            }

            public string GetCorrelationFromScoped => _correlationContext.CorrelationContext.CorrelationId;
        }

        private class ScopedClass
        {
            private readonly ICorrelationContextAccessor _correlationContext;

            public ScopedClass(ICorrelationContextAccessor correlationContext)
            {
                _correlationContext = correlationContext;
            }

            public string GetCorrelationFromScoped => _correlationContext.CorrelationContext.CorrelationId;
        }

        private class TransientClass
        {
            private readonly ICorrelationContextAccessor _correlationContext;

            public TransientClass(ICorrelationContextAccessor correlationContext)
            {
                _correlationContext = correlationContext;
            }

            public string GetCorrelationFromScoped => _correlationContext.CorrelationContext.CorrelationId;
        }
    }
}
