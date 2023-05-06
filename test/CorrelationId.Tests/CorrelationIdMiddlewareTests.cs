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
using System.Net;
using CorrelationId.Abstractions;
using CorrelationId.DependencyInjection;
using CorrelationId.HttpClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace CorrelationId.Tests
{
    public class CorrelationIdMiddlewareTests
    {
        [Fact]
        public async Task Throws_WhenCorrelationIdProviderIsNotRegistered()
        {
            Exception exception = null;

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Use(async (ctx, next) =>
                    {
                        try
                        {
                            await next.Invoke();
                        }
                        catch (Exception e)
                        {
                            exception = e;
                        }                        
                    });

                    app.UseCorrelationId();
                })
                .ConfigureServices(sc => sc.AddCorrelationId());

            using var server = new TestServer(builder);

            await server.CreateClient().GetAsync("");

            Assert.NotNull(exception);
            Assert.Equal(typeof(InvalidOperationException), exception.GetType());
        }

        [Fact]
        public async Task DoesNotThrow_WhenCorrelationIdProviderIsRegistered()
        {
            Exception exception = null;

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Use(async (ctx, next) =>
                    {
                        try
                        {
                            await next.Invoke();
                        }
                        catch (Exception e)
                        {
                            exception = e;
                        }
                    });

                    app.UseCorrelationId();
                })
                .ConfigureServices(sc => sc.AddDefaultCorrelationId());

            using var server = new TestServer(builder);

            await server.CreateClient().GetAsync("");

            Assert.Null(exception);
        }

        [Fact]
        public async Task ReturnsBadRequest_WhenEnforceOptionSetToTrue_AndNoExistingHeaderIsSent()
        {
            var builder = new WebHostBuilder()
                .Configure(app => app.UseCorrelationId())
                .ConfigureServices(sc => sc.AddDefaultCorrelationId(options => options.EnforceHeader = true));

            using var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DoesNotReturnBadRequest_WhenEnforceOptionSetToFalse_AndNoExistingHeaderIsSent()
        {
            var builder = new WebHostBuilder()
                .Configure(app => app.UseCorrelationId())
                .ConfigureServices(sc => sc.AddDefaultCorrelationId(options => options.EnforceHeader = false));

            using var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task IgnoresRequestHeader_WhenOptionIsTrue()
        {
            var builder = new WebHostBuilder()
                .Configure(app => app.UseCorrelationId())
                .ConfigureServices(sc => sc.AddDefaultCorrelationId(options => { options.IgnoreRequestHeader = true; options.IncludeInResponse = true; }));

            using var server = new TestServer(builder);

            var request = new HttpRequestMessage();
            request.Headers.Add(CorrelationIdOptions.DefaultHeader, "ABC123");

            var response = await server.CreateClient().SendAsync(request);

            var header = response.Headers.GetValues(CorrelationIdOptions.DefaultHeader).FirstOrDefault();

            Assert.NotEqual("ABC123", header);
        }

        [Fact]
        public async Task DoesNotIgnoresRequestHeader_WhenOptionIsFalse()
        {
            var builder = new WebHostBuilder()
                .Configure(app => app.UseCorrelationId())
                .ConfigureServices(sc => sc.AddDefaultCorrelationId(options => { options.IgnoreRequestHeader = false; options.IncludeInResponse = true; }));

            using var server = new TestServer(builder);

            var request = new HttpRequestMessage();
            request.Headers.Add(CorrelationIdOptions.DefaultHeader, "ABC123");

            var response = await server.CreateClient().SendAsync(request);

            var header = response.Headers.GetValues(CorrelationIdOptions.DefaultHeader).FirstOrDefault();

            Assert.Equal("ABC123", header);
        }

        [Fact]
        public async Task ReturnsCorrelationIdInResponseHeader_WhenOptionSetToTrue()
        {
            var builder = new WebHostBuilder()
               .Configure(app => app.UseCorrelationId())
               .ConfigureServices(sc => sc.AddDefaultCorrelationId(options => options.IncludeInResponse = true));

            using var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");
            
            var header = response.Headers.GetValues(CorrelationIdOptions.DefaultHeader);

            Assert.NotNull(header);
        }

        [Fact]
        public async Task DoesNotThrowException_WhenOptionSetToTrue_IfHeaderIsAlreadySet()
        {
            Exception exception = null;

            var builder = new WebHostBuilder()
               .Configure(app =>
               {
                   app.Use(async (ctx, next) =>
                   {
                       try
                       {
                           await next.Invoke();
                       }
                       catch (Exception e)
                       {
                           exception = e;
                       }
                   });

                   app.UseCorrelationId();
                   app.UseCorrelationId(); // header will already be set on this second use of the middleware
               })
               .ConfigureServices(sc => sc.AddDefaultCorrelationId());
            
            using var server = new TestServer(builder);

            await server.CreateClient().GetAsync("");

            Assert.Null(exception);
        }

        [Fact]
        public async Task DoesNotReturnCorrelationIdInResponseHeader_WhenIncludeInResponseIsFalse()
        {
            string header = null;

            var builder = new WebHostBuilder()
               .Configure(app => app.UseCorrelationId())
               .ConfigureServices(sc => sc.AddDefaultCorrelationId(options =>
               {
                   options.IncludeInResponse = false;
                   header = options.RequestHeader;
               }));

            using var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            var headerExists = response.Headers.TryGetValues(header, out IEnumerable<string> _);

            Assert.False(headerExists);
        }

        [Fact]
        public async Task CorrelationIdHeaderFieldName_MatchesHeaderOption()
        {
            const string customHeader = "X-Test-RequestHeader";

            var builder = new WebHostBuilder()
               .Configure(app => app.UseCorrelationId())
               .ConfigureServices(sc => sc.AddDefaultCorrelationId(options => options.ResponseHeader = customHeader));

            using var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            var header = response.Headers.GetValues(customHeader);

            Assert.NotNull(header);
        }

        [Fact]
        public async Task CorrelationId_SetToCorrelationIdFromRequestHeader()
        {
            var expectedHeaderName = new CorrelationIdOptions().RequestHeader;
            const string expectedHeaderValue = "123456";

            var builder = new WebHostBuilder()
               .Configure(app => app.UseCorrelationId())
               .ConfigureServices(sc => sc.AddDefaultCorrelationId());

            using var server = new TestServer(builder);

            var request = new HttpRequestMessage();
            request.Headers.Add(expectedHeaderName, expectedHeaderValue);

            var response = await server.CreateClient().SendAsync(request);

            var header = response.Headers.GetValues(expectedHeaderName);

            Assert.Single(header, expectedHeaderValue);
        }
        
        [Fact]
        public async Task CorrelationId_UpdatesRequestWithtGeneratedCorrelationIdWhenNotGiven()
        {
            var expectedHeaderName = new CorrelationIdOptions().RequestHeader;
            StringValues excectedRequestValue;
            
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseCorrelationId();
                    app.Use((HttpContext ctx, Func<Task> _) =>
                    {
                        ctx.Request.Headers.TryGetValue(expectedHeaderName, out excectedRequestValue);
                        return Task.CompletedTask;
                    });
                })
                
                .ConfigureServices(sc => sc.AddDefaultCorrelationId());

            using var server = new TestServer(builder);

            var request = new HttpRequestMessage();

            await server.CreateClient().SendAsync(request);

            Assert.Single(excectedRequestValue);
            Assert.True(Guid.TryParse(excectedRequestValue.Single(), out _));
        }

        [Fact]
        public async Task CorrelationId_ReturnsGeneratedCorrelationIdWhenNotGiven()
        {
            var expectedHeaderName = new CorrelationIdOptions().RequestHeader;

            var builder = new WebHostBuilder()
                .Configure(app => app.UseCorrelationId())
                .ConfigureServices(sc => sc.AddDefaultCorrelationId());

            using var server = new TestServer(builder);

            var request = new HttpRequestMessage();

            var response = await server.CreateClient().SendAsync(request);

            var header = response.Headers.GetValues(expectedHeaderName)
                .ToList();

            Assert.Single(header);
            Assert.True(Guid.TryParse(header.Single(), out _));
        }
        
        [Fact]
        public async Task CorrelationId_SetToGuid_RegisteredWithAddDefaultCorrelationId()
        {
            var builder = new WebHostBuilder()
                .Configure(app => app.UseCorrelationId())
                .ConfigureServices(sc => sc.AddDefaultCorrelationId());

            using var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            var header = response.Headers.GetValues(new CorrelationIdOptions().RequestHeader);

            var isGuid = Guid.TryParse(header.FirstOrDefault(), out _);

            Assert.True(isGuid);
        }

        [Fact]
        public async Task CorrelationId_SetToCustomGenerator_WhenCorrelationIdGeneratorIsSet()
        {
            var builder = new WebHostBuilder()
                .Configure(app => app.UseCorrelationId())
                .ConfigureServices(sc => sc.AddDefaultCorrelationId(options => options.CorrelationIdGenerator = () => "Foo"));

            using var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            var header = response.Headers.GetValues(new CorrelationIdOptions().RequestHeader);

            var correlationId = header.FirstOrDefault();

            Assert.Equal("Foo", correlationId);
        }

        [Fact]
        public async Task CorrelationId_NotSetToGuid_WhenUsingTheTraceIdentifierProvider()
        {
            var builder = new WebHostBuilder()
                .Configure(app => app.UseCorrelationId())
                .ConfigureServices(sc => sc.AddCorrelationId().WithTraceIdentifierProvider());

            using var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            var header = response.Headers.GetValues(new CorrelationIdOptions().RequestHeader);

            var isGuid = Guid.TryParse(header.FirstOrDefault(), out _);

            Assert.False(isGuid);
        }

        [Fact]
        public async Task CorrelationId_SetUsingCustomProvider_WhenCustomProviderIsRegistered()
        {
            var builder = new WebHostBuilder()
                .Configure(app => app.UseCorrelationId())
                .ConfigureServices(sc => sc.AddCorrelationId().WithCustomProvider<TestCorrelationIdProvider>());

            using var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            var header = response.Headers.GetValues(new CorrelationIdOptions().RequestHeader);

            Assert.Equal(TestCorrelationIdProvider.FixedCorrelationId, header.FirstOrDefault());
        }

        [Fact]
        public async Task CorrelationId_ReturnedCorrectlyFromSingletonService()
        {
            var expectedHeaderName = new CorrelationIdOptions().RequestHeader;

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
                    sc.AddDefaultCorrelationId();
                    sc.TryAddSingleton<SingletonClass>();
                    sc.TryAddScoped<ScopedClass>();
                });

            using var server = new TestServer(builder);

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
            var expectedHeaderName = new CorrelationIdOptions().RequestHeader;

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
                    sc.AddDefaultCorrelationId();
                    sc.TryAddTransient<TransientClass>();
                    sc.TryAddScoped<ScopedClass>();
                });

            using var server = new TestServer(builder);

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
            const string customHeader = "custom-header";

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseCorrelationId();

                    app.Use(async (HttpContext ctx, Func<Task> _) =>
                    {
                        var accessor = ctx.RequestServices.GetService<ICorrelationContextAccessor>();
                        ctx.Response.StatusCode = StatusCodes.Status200OK;
                        await ctx.Response.WriteAsync(accessor.CorrelationContext.Header);
                    });
                })
                .ConfigureServices(sc => sc.AddDefaultCorrelationId(options => options.RequestHeader = customHeader));

            using var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync("");

            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(customHeader, body);
        }

        [Fact]
        public async Task TraceIdentifier_IsNotUpdated_WhenUpdateTraceIdentifierIsFalse()
        {
            string originalTraceIdentifier = null;
            string traceIdentifier = null;

            const string correlationId = "123456";

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Use(async (ctx, next) =>
                    {
                        originalTraceIdentifier = ctx.TraceIdentifier;
                        await next.Invoke();
                    });

                    app.UseCorrelationId();

                    app.Use((HttpContext ctx, Func<Task> _) =>
                    {
                        traceIdentifier = ctx.TraceIdentifier;
                        return Task.CompletedTask;
                    });
                })
                .ConfigureServices(sc => sc.AddDefaultCorrelationId(options => options.UpdateTraceIdentifier = false));

            using var server = new TestServer(builder);

            var request = new HttpRequestMessage();
            request.Headers.Add(CorrelationIdOptions.DefaultHeader, correlationId);

            await server.CreateClient().SendAsync(request);
            
            Assert.Equal(originalTraceIdentifier, traceIdentifier);
        }

        [Fact]
        public async Task TraceIdentifier_IsNotUpdated_WhenUpdateTraceIdentifierIsTrue_ButIncomingCorrelationIdIsEmpty()
        {
            string originalTraceIdentifier = null;
            string traceIdentifier = null;
            
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Use(async (ctx, next) =>
                    {
                        originalTraceIdentifier = ctx.TraceIdentifier;
                        await next.Invoke();
                    });

                    app.UseCorrelationId();

                    app.Use((HttpContext ctx, Func<Task> _) =>
                    {
                        traceIdentifier = ctx.TraceIdentifier;
                        return Task.CompletedTask;
                    });
                })
                .ConfigureServices(sc => sc.AddDefaultCorrelationId(options => options.UpdateTraceIdentifier = true));

            using var server = new TestServer(builder);

            var request = new HttpRequestMessage();
            request.Headers.Add(CorrelationIdOptions.DefaultHeader, "");

            await server.CreateClient().SendAsync(request);

            Assert.NotEqual(originalTraceIdentifier, traceIdentifier);
        }

        [Fact]
        public async Task TraceIdentifier_IsNotUpdated_WhenUpdateTraceIdentifierIsTrue_AndGeneratedCorrelationIdIsNull()
        {
            string originalTraceIdentifier = null;
            string traceIdentifier = null;

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.Use(async (ctx, next) =>
                    {
                        originalTraceIdentifier = ctx.TraceIdentifier;
                        await next.Invoke();
                    });

                    app.UseCorrelationId();

                    app.Use((HttpContext ctx, Func<Task> _) =>
                    {
                        traceIdentifier = ctx.TraceIdentifier;
                        return Task.CompletedTask;
                    });
                })
                .ConfigureServices(sc => sc.AddDefaultCorrelationId(options => { options.UpdateTraceIdentifier = true; options.CorrelationIdGenerator = () => null; }));

            using var server = new TestServer(builder);
            
            await server.CreateClient().GetAsync("");

            Assert.Equal(originalTraceIdentifier, traceIdentifier);
        }

        [Fact]
        public async Task TraceIdentifier_IsUpdated_WhenUpdateTraceIdentifierIsTrue()
        {
            string traceIdentifier = null;

            var expectedHeaderName = new CorrelationIdOptions().RequestHeader;
            const string correlationId = "123456";

            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseCorrelationId();

                    app.Use((HttpContext ctx, Func<Task> _) =>
                    {
                        traceIdentifier = ctx.TraceIdentifier;
                        return Task.CompletedTask;
                    });
                })
                .ConfigureServices(sc => sc.AddDefaultCorrelationId(options => options.UpdateTraceIdentifier = true));

            using var server = new TestServer(builder);

            var request = new HttpRequestMessage();
            request.Headers.Add(expectedHeaderName, correlationId);

            await server.CreateClient().SendAsync(request);
            
            Assert.Equal(correlationId, traceIdentifier);
        }
        
        [Fact]
        public async Task NestedHttpClient_ShouldReturnSameCorrelationId()
        {
            var expectedHeaderName = new CorrelationIdOptions().RequestHeader;
            const string correlationId = "123456";

            var innerBuilder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseCorrelationId();
                    app.Use((HttpContext _, Func<Task> _) => Task.CompletedTask);
                })
                .ConfigureServices(sc => sc.AddDefaultCorrelationId());

            using var innerServer = new TestServer(innerBuilder);
            var innerHandler = innerServer.CreateHandler();
            
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseCorrelationId();
                    app.Use((HttpContext ctx, Func<Task> _) =>
                        Task.Run(async () =>
                        {
                            var httpClientFactory = ctx.RequestServices.GetService<IHttpClientFactory>();
                            var client = httpClientFactory
                                    .CreateClient("MyClient"); // this client will attach the correlation ID header

                            var innerResponse = await client.GetAsync("https://www.example.com");

                            var innerResponseCorrelationId =
                                innerResponse.Headers.GetValues(new CorrelationIdOptions().RequestHeader).Single();
                            
                            Assert.Equal(correlationId, innerResponseCorrelationId);
                        }));
                })
                .ConfigureServices(serviceCollection =>
                {
                    serviceCollection.AddLogging(logging =>
                    {
                        logging.AddDebug();
                        logging.SetMinimumLevel(LogLevel.Trace);
                    });
                    serviceCollection.AddDefaultCorrelationId();
                    serviceCollection.AddHttpClient("MyClient")
                        .ConfigurePrimaryHttpMessageHandler(_ => innerHandler).AddCorrelationIdForwarding();
                });

            using var server = new TestServer(builder);

            var request = new HttpRequestMessage();
            request.Headers.Add(expectedHeaderName, correlationId);

            var response = await server.CreateClient().SendAsync(request);
            
            var header = response.Headers.GetValues(expectedHeaderName);

            Assert.Single(header, correlationId);

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

        private class TestCorrelationIdProvider : ICorrelationIdProvider
        {
            public const string FixedCorrelationId = "TestCorrelationId";

            public string GenerateCorrelationId(HttpContext context) => FixedCorrelationId;
        }

    }
}
