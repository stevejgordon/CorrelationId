using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CorrelationId;
using CorrelationId.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AspnetHttpContext = Microsoft.AspNetCore.Http.HttpContext;
using AspnetDefaultHttpContext = Microsoft.AspNetCore.Http.DefaultHttpContext;

namespace Net48CorrelationId
{
    /// <summary>
    /// We don't have any middleware in .net framework so we wrap it in a HttpMessageHandler instead
    /// </summary>
    public class CorrelationIdMiddlewareWrapper : DelegatingHandler
    {
        private readonly ICorrelationContextFactory _correlationContextFactory;
        private readonly IOptions<CorrelationIdOptions> _correlationIdOptions;
        private readonly ILogger<CorrelationIdMiddlewareWrapper> _correlationIdMiddlewareLoggerWrapper;
        private readonly ILogger<CorrelationIdMiddleware> _correlationIdMiddlewareLogger;
        private readonly ICorrelationIdProvider _correlationIdProvider;
        private readonly ICorrelationContextAccessor _correlationContextAccessor;

        public CorrelationIdMiddlewareWrapper(ICorrelationContextFactory correlationContextFactory,
            IOptions<CorrelationIdOptions> correlationIdOptions,
            ILogger<CorrelationIdMiddlewareWrapper> correlationIdMiddlewareLoggerWrapper,
            // ReSharper disable once ContextualLoggerProblem
            ILogger<CorrelationIdMiddleware> correlationIdMiddlewareLogger,
            ICorrelationIdProvider correlationIdProvider, ICorrelationContextAccessor correlationContextAccessor)
        {
            _correlationContextFactory = correlationContextFactory;
            _correlationIdOptions = correlationIdOptions;
            _correlationIdMiddlewareLoggerWrapper = correlationIdMiddlewareLoggerWrapper;
            _correlationIdMiddlewareLogger = correlationIdMiddlewareLogger;
            _correlationIdProvider = correlationIdProvider;
            _correlationContextAccessor = correlationContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            var correlationIdMiddleware = new CorrelationIdMiddleware(
                context => MiddlewareActionAsync(context, request, x => response = x), _correlationIdMiddlewareLogger,
                _correlationIdOptions, _correlationIdProvider);
            var aspnetHttpContext = GetAspnetHttpContext();

            if (_correlationContextAccessor.CorrelationContext == null)
            {
                // we should just do this to setup the context if missing
                await correlationIdMiddleware.Invoke(aspnetHttpContext, _correlationContextFactory);
            }
            else
            {
                await MiddlewareActionAsync(aspnetHttpContext, request, x => response = x);
            }

            return response;
        }

        private async Task MiddlewareActionAsync(AspnetHttpContext aspnetHttpContext,
            HttpRequestMessage request, Action<HttpResponseMessage> responseSetter)
        {
            var correlationId = _correlationContextAccessor?.CorrelationContext?.CorrelationId;
            if (!string.IsNullOrEmpty(correlationId)
                && !request.Headers.Contains(_correlationContextAccessor.CorrelationContext.Header))
            {
                request.Headers.Add(_correlationContextAccessor.CorrelationContext.Header,
                    _correlationContextAccessor.CorrelationContext.CorrelationId);
            }

            var response = await base.SendAsync(request, aspnetHttpContext.RequestAborted);

            // context.Response.OnStarting is not executed on .net FW
            if (_correlationIdOptions.Value.IncludeInResponse &&
                !string.IsNullOrEmpty(correlationId) &&
                !response.Headers.Contains(_correlationIdOptions.Value.ResponseHeader))
            {
                _correlationIdMiddlewareLoggerWrapper.LogDebug(
                    "Writing correlation ID response header {ResponseHeader} with value {CorrelationId}",
                    _correlationIdOptions.Value.ResponseHeader, correlationId);
                response.Headers.Add(_correlationIdOptions.Value.ResponseHeader, correlationId);
            }
            
            responseSetter.Invoke(response);
        }

        private AspnetHttpContext GetAspnetHttpContext()
        {
            var httpContext = new AspnetDefaultHttpContext();

            if (System.Web.HttpContext.Current == null)
            {
                return httpContext;
            }

            var correlationIds =
                System.Web.HttpContext.Current.Request.Headers
                    .GetValues(_correlationIdOptions.Value.RequestHeader);

            if (correlationIds == null)
            {
                return httpContext;
            }

            foreach (var correlationId in correlationIds)
            {
                httpContext.Request.Headers
                    .Add(_correlationIdOptions.Value.RequestHeader, correlationId);
            }

            return httpContext;
        }
    }
}